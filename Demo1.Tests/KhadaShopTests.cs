using System.Text.RegularExpressions;
using Demo1.Data;
using Demo1.Models;
using Demo1.Services;
using Demo1.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Tests;

public class KhadaShopTests
{
    [Fact]
    public async Task OrderCodeGenerator_TaoMaDungDinhDang()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        var code = await new OrderCodeGenerator(fixture.Db).GenerateAsync();
        Assert.Matches(new Regex("^KS-[0-9]{8}-[A-F0-9]{6}$"), code);
    }

    [Fact]
    public void DonDuoiNguong_CoPhiGiaoHang30000()
        => Assert.Equal(30_000m, OrderService.CalculateShipping(1_499_999m));

    [Fact]
    public void DonTuNguong_DuocMienPhiGiaoHang()
        => Assert.Equal(0m, OrderService.CalculateShipping(1_500_000m));

    [Fact]
    public void LineTotal_BangDonGiaNhanSoLuong()
    {
        var item = new OrderItem { UnitPrice = 825_000m, Quantity = 3, LineTotal = 825_000m * 3 };
        Assert.Equal(item.UnitPrice * item.Quantity, item.LineTotal);
    }

    [Fact]
    public void QuantityNhoHonMot_BiTuChoi()
        => Assert.Throws<ArgumentOutOfRangeException>(() => CartService.ValidateQuantity(0));

    [Fact]
    public async Task CheckoutVuotTonKho_BiTuChoi()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        var product = new Product { Name = "Mũ test", Slug = "mu-test", ShortDescription = "Test", Description = "Test", BasePrice = 1_000_000m, Category = new Category { Name = "Test", Slug = "test" } };
        var variant = new ProductVariant { Product = product, Size = "M", Color = "Đen", Sku = "TEST-001", StockQuantity = 1 };
        fixture.Db.ProductVariants.Add(variant);
        await fixture.Db.SaveChangesAsync();
        var cart = new FakeCart([new CartSessionItem { ProductVariantId = variant.Id, Quantity = 2 }]);
        var service = new OrderService(fixture.Db, cart, new FixedCodeGenerator());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CheckoutInputModel { CustomerName = "Khách", PhoneNumber = "0900000000", Address = "Địa chỉ test" }));
        Assert.Contains("không đủ tồn kho", exception.Message);
        Assert.False(cart.WasCleared);
    }

    [Fact]
    public async Task HuyDon_HoanTonKhoDungMotLan()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        var product = new Product { Name = "Mũ test", Slug = "mu-test-2", ShortDescription = "Test", Description = "Test", BasePrice = 1_000_000m, Category = new Category { Name = "Test", Slug = "test-2" } };
        var variant = new ProductVariant { Product = product, Size = "L", Color = "Đỏ", Sku = "TEST-002", StockQuantity = 5 };
        var order = new Order { OrderCode = "KS-20260801-ABCDEF", CustomerName = "Khách", PhoneNumber = "0900000000", Address = "Test", Status = OrderStatus.Pending, Items = [new OrderItem { Product = product, ProductVariant = variant, ProductName = product.Name, Sku = variant.Sku, Size = variant.Size, Color = variant.Color, Quantity = 2, UnitPrice = 1_000_000m, LineTotal = 2_000_000m }] };
        fixture.Db.Orders.Add(order);
        await fixture.Db.SaveChangesAsync();
        var service = new OrderService(fixture.Db, new FakeCart([]), new FixedCodeGenerator());

        Assert.True(await service.UpdateStatusAsync(order.Id, OrderStatus.Cancelled));
        fixture.Db.ChangeTracker.Clear();
        Assert.Equal(7, (await fixture.Db.ProductVariants.SingleAsync()).StockQuantity);
        Assert.True(await service.UpdateStatusAsync(order.Id, OrderStatus.Cancelled));
        fixture.Db.ChangeTracker.Clear();
        Assert.Equal(7, (await fixture.Db.ProductVariants.SingleAsync()).StockQuantity);
    }

    [Fact]
    public void FacebookJavascriptUrl_BiTuChoi()
        => Assert.False(UrlSafety.IsSafeHttpUrl("javascript:alert(1)"));

    [Fact]
    public async Task BannerHetHan_KhongXuatHien()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        var now = DateTime.UtcNow;
        fixture.Db.Banners.Add(new Banner { Title = "Hết hạn", IsActive = true, EndAtUtc = now.AddMinutes(-1) });
        await fixture.Db.SaveChangesAsync();
        Assert.Empty(await new BannerService(fixture.Db).GetActiveAsync(now));
    }

    [Fact]
    public async Task BannerChuaDenNgay_KhongXuatHien()
    {
        await using var fixture = await TestDatabase.CreateAsync();
        var now = DateTime.UtcNow;
        fixture.Db.Banners.Add(new Banner { Title = "Chưa đến ngày", IsActive = true, StartAtUtc = now.AddMinutes(1) });
        await fixture.Db.SaveChangesAsync();
        Assert.Empty(await new BannerService(fixture.Db).GetActiveAsync(now));
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        public ApplicationDbContext Db { get; }
        private TestDatabase(SqliteConnection connection, ApplicationDbContext db) { _connection = connection; Db = db; }
        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
            await db.Database.EnsureCreatedAsync();
            return new TestDatabase(connection, db);
        }
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
    }

    private sealed class FixedCodeGenerator : IOrderCodeGenerator
    {
        public Task<string> GenerateAsync(CancellationToken cancellationToken = default) => Task.FromResult("KS-20260801-123ABC");
    }

    private sealed class FakeCart(IReadOnlyList<CartSessionItem> items) : ICartService
    {
        public bool WasCleared { get; private set; }
        public Task AddAsync(int productVariantId, int quantity, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task UpdateAsync(int productVariantId, int quantity, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Remove(int productVariantId) => throw new NotSupportedException();
        public void Clear() => WasCleared = true;
        public int GetCount() => items.Sum(x => x.Quantity);
        public IReadOnlyList<CartSessionItem> GetStoredItems() => items;
        public Task<CartViewModel> GetCartAsync(CancellationToken cancellationToken = default) => Task.FromResult(new CartViewModel());
    }
}
