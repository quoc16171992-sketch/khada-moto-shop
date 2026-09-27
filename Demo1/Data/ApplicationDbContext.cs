using Demo1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductGalleryImage> ProductGalleryImages => Set<ProductGalleryImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<ProductVariant>().HasIndex(x => x.Sku).IsUnique();
        builder.Entity<ProductVariant>().HasIndex(x => new { x.ProductId, x.Size, x.Color }).IsUnique();
        builder.Entity<ProductGalleryImage>().HasIndex(x => new { x.ProductId, x.DisplayOrder });
        builder.Entity<Order>().HasIndex(x => x.OrderCode).IsUnique();

        builder.Entity<Product>().Property(x => x.BasePrice).HasPrecision(18, 2);
        builder.Entity<Product>().Property(x => x.CompareAtPrice).HasPrecision(18, 2);
        builder.Entity<ProductVariant>().Property(x => x.PriceAdjustment).HasPrecision(18, 2);
        builder.Entity<Order>().Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Entity<Order>().Property(x => x.ShippingFee).HasPrecision(18, 2);
        builder.Entity<Order>().Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Entity<OrderItem>().Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.Entity<OrderItem>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<OrderItem>()
            .HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Order>()
            .HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Product>()
            .HasMany(x => x.GalleryImages)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
