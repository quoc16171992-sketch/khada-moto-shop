using Demo1.Data;
using Demo1.Models;
using Demo1.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Services;

public class OrderService(ApplicationDbContext db, ICartService cart, IOrderCodeGenerator codeGenerator) : IOrderService
{
    public const decimal FreeShippingThreshold = 1_500_000m;
    public const decimal StandardShippingFee = 30_000m;

    public decimal CalculateShippingFee(decimal subtotal) => CalculateShipping(subtotal);
    public static decimal CalculateShipping(decimal subtotal) => subtotal >= FreeShippingThreshold ? 0m : StandardShippingFee;

    public async Task<Order> CreateAsync(CheckoutInputModel input, CancellationToken cancellationToken = default)
    {
        var storedItems = cart.GetStoredItems();
        if (storedItems.Count == 0)
        {
            throw new InvalidOperationException("Giỏ hàng đang trống.");
        }

        var grouped = storedItems.GroupBy(x => x.ProductVariantId)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var variantIds = grouped.Keys.ToArray();
            var variants = await db.ProductVariants.Include(x => x.Product)
                .Where(x => variantIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);

            if (variants.Count != grouped.Count)
            {
                throw new InvalidOperationException("Một sản phẩm trong giỏ không còn tồn tại.");
            }

            var orderItems = new List<OrderItem>();
            foreach (var (variantId, quantity) in grouped)
            {
                CartService.ValidateQuantity(quantity);
                var variant = variants[variantId];
                if (!variant.IsActive || !variant.Product.IsActive)
                {
                    throw new InvalidOperationException($"{variant.Product.Name} hiện không còn bán.");
                }
                if (quantity > variant.StockQuantity)
                {
                    throw new InvalidOperationException($"{variant.Product.Name} không đủ tồn kho.");
                }

                var unitPrice = variant.Product.BasePrice + variant.PriceAdjustment;
                orderItems.Add(new OrderItem
                {
                    ProductId = variant.ProductId,
                    ProductVariantId = variant.Id,
                    ProductName = variant.Product.Name,
                    Sku = variant.Sku,
                    Size = variant.Size,
                    Color = variant.Color,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    LineTotal = unitPrice * quantity
                });
                variant.StockQuantity -= quantity;
            }

            var subtotal = orderItems.Sum(x => x.LineTotal);
            var shippingFee = CalculateShipping(subtotal);
            var order = new Order
            {
                OrderCode = await codeGenerator.GenerateAsync(cancellationToken),
                CustomerName = input.CustomerName.Trim(),
                PhoneNumber = input.PhoneNumber.Trim(),
                Email = input.Email?.Trim(),
                Address = input.Address.Trim(),
                Note = input.Note?.Trim(),
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                TotalAmount = subtotal + shippingFee,
                Status = OrderStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow,
                Items = orderItems
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            cart.Clear();
            return order;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);
            if (order is null || (order.Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled))
            {
                return false;
            }

            if (status == OrderStatus.Cancelled && !order.StockRestored)
            {
                var variantIds = order.Items.Where(x => x.ProductVariantId.HasValue)
                    .Select(x => x.ProductVariantId!.Value).Distinct().ToArray();
                var variants = await db.ProductVariants.Where(x => variantIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);
                foreach (var item in order.Items)
                {
                    if (item.ProductVariantId.HasValue && variants.TryGetValue(item.ProductVariantId.Value, out var variant))
                    {
                        variant.StockQuantity += item.Quantity;
                    }
                }
                order.StockRestored = true;
            }

            order.Status = status;
            order.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
