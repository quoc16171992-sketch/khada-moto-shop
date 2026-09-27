using Demo1.Models;
using Demo1.ViewModels;

namespace Demo1.Services;

public interface IOrderService
{
    decimal CalculateShippingFee(decimal subtotal);
    Task<Order> CreateAsync(CheckoutInputModel input, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);
}
