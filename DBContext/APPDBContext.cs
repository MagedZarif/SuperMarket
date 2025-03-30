using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperMarket.models;
 

//Fluent API
//data annotations

namespace SuperMarket.DBContext
{
    public class APPDBContext : IdentityDbContext<IdentityUser>
    {

        public APPDBContext(DbContextOptions<APPDBContext> options) : base(options)
        {

        }
        public DbSet<Category> categories { get; set; }
        public DbSet<Item> items { get; set; }
        public DbSet<IItem> Iitems { get; set; }
        public DbSet<Sale> sales { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            modelBuilder.Entity<Item>()
                .HasMany(i => i.Iitems)
                .WithOne(i => i.Item)
                .HasForeignKey(i => i.ItemId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Sale>()
                .HasMany(s => s.Iitems)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<Item>()
            //    .Ignore(i => i.Quantity)
            //    .Ignore(i => i.IsExpired);
        }



        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            var modifiedItemIds = ChangeTracker.Entries<IItem>()
         .Where(e => e.State == EntityState.Added || e.State == EntityState.Deleted||e.State==EntityState.Modified)
         .Select(e => e.Entity.ItemId)
         .Distinct()
         .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            if (modifiedItemIds.Any())
            {
                foreach (var itemId in modifiedItemIds)
                {
                    var item = await items.FindAsync(itemId);
                    if (item != null)
                    {
                        item.Quantity = await Iitems.CountAsync(i => i.ItemId == itemId);
                        item.IsExpired = await Iitems.AnyAsync(i => i.ItemId == itemId && i.ExpiredDate < DateTime.UtcNow);
                    }
                }
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
    }
}
