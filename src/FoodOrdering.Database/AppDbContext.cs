using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodOrdering.Domain.Entities;
using FoodOrdering.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdering.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TblUser> Users => Set<TblUser>();
        public DbSet<TblRefreshToken> RefreshTokens => Set<TblRefreshToken>();
        public DbSet<TblFoodCategory> FoodCategories => Set<TblFoodCategory>();
        public DbSet<TblFoodItem> FoodItems => Set<TblFoodItem>();
        public DbSet<TblTable> Tables => Set<TblTable>();
        public DbSet<TblCart> Carts => Set<TblCart>();
        public DbSet<TblCartItem> CartItems => Set<TblCartItem>();
        public DbSet<TblSale> Sales => Set<TblSale>();
        public DbSet<TblSaleDetail> SaleDetails => Set<TblSaleDetail>();
        public DbSet<TblReservation> Reservations => Set<TblReservation>();
        public DbSet<TblNotification> Notifications => Set<TblNotification>();
        public DbSet<TblPromotion> Promotions => Set<TblPromotion>();
        public DbSet<TblWishlist> Wishlists => Set<TblWishlist>();
        public DbSet<TblDelivery> Deliveries => Set<TblDelivery>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Soft-Delete Query Filters
            modelBuilder.Entity<TblUser>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<TblFoodCategory>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<TblFoodItem>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<TblTable>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<TblCart>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<TblSale>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<TblReservation>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<TblNotification>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<TblPromotion>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<TblDelivery>().HasQueryFilter(e => !e.IsDelete);

            // Relationships & Precision
            modelBuilder.Entity<TblUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<TblFoodItem>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TblFoodItem>()
                .Property(p => p.DeliveryFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TblSale>()
                .Property(p => p.SubTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TblSale>()
                .Property(p => p.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TblSale>()
                .Property(p => p.DeliveryFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TblSale>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TblSaleDetail>()
                .Property(p => p.PricePerItem)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TblSaleDetail>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TblPromotion>()
                .Property(p => p.Value)
                .HasPrecision(18, 2);

            // Unique voucher number
            modelBuilder.Entity<TblSale>()
                .HasIndex(s => s.VoucherNo)
                .IsUnique();

            // Cascade rules
            modelBuilder.Entity<TblCartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TblSaleDetail>()
                .HasOne(sd => sd.Sale)
                .WithMany(s => s.SaleDetails)
                .HasForeignKey(sd => sd.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChanges();
        }

        private void ApplyAuditAndSoftDelete()
        {
            // Myanmar Time offset (UTC + 6:30)
            var mmTime = DateTime.UtcNow.AddHours(6).AddMinutes(30);

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is BaseEntity entity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entity.CreatedAt = mmTime;
                            entity.IsDelete = false;
                            break;

                        case EntityState.Modified:
                            entity.UpdatedAt = mmTime;
                            break;

                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            entity.IsDelete = true;
                            entity.UpdatedAt = mmTime;
                            break;
                    }
                }
            }
        }
    }
}
