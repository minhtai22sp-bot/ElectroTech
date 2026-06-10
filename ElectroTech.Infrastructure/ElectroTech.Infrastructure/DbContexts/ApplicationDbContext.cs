using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Interfaces;
using AspNetCoreHero.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;

namespace ElectroTech.Infrastructure.DbContexts
{
    public partial class ApplicationDbContext : AuditableLogContext, IApplicationDbContext
    {
        private readonly IDateTimeService _dateTime;
        private readonly IAuthenticatedUserService _authenticatedUser;
        private IDbContextTransaction _transaction;
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
             ILogger<ApplicationDbContext> logger,
            IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser) : base(options)
        {
            _logger = logger;
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }
        public IDbConnection Connection => throw new NotImplementedException();
        public bool HasChanges => ChangeTracker.HasChanges();
        public DbSet<Product> Products { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSpec> ProductSpecs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public void BeginTransaction()
        {
            _transaction = Database.BeginTransaction();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        //entry.Entity.CreatedOn = _dateTime.NowUtc;
                        entry.Entity.CreatedOn = _dateTime.Now;
                        entry.Entity.CreatedBy = _authenticatedUser.UserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedOn = _dateTime.Now;
                        entry.Entity.LastModifiedBy = _authenticatedUser.UserId;
                        break;
                }
            }
            // int resurlt = 0;
            if (_authenticatedUser.UserId == null)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            else
            {
                return await base.SaveChangesAsync(_authenticatedUser.ComId, _authenticatedUser.UserId);
                //return await base.SaveChangesAsync(_authenticatedUser.UserId);
            }
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            // quan hệ 1-1 https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-one
            foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,3)");
            }


            builder.Entity<Product>(entity =>
            {
                // Tạo Index Unique trên 1 cột
                entity.HasIndex(p => new { p.Code }).IsUnique();
                entity.HasOne(x => x.Categories).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId);
                entity.HasMany(x => x.ProductSpecs).WithOne(x => x.Product).HasForeignKey(x => x.ProductId);
                entity.HasMany(x => x.Reviews).WithOne(x => x.Product).HasForeignKey(x => x.ProductId);
            });
            
            builder.Entity<Order>(entity =>
            {
                // Tạo Index Unique trên 1 cột
                entity.HasIndex(p => new { p.OrderCode }).IsUnique();
                entity.HasMany(x => x.OrderItems).WithOne(x => x.Order).HasForeignKey(x => x.OrderId);
                entity.HasOne(x => x.Coupon);
            });

            //builder.Entity<ManagerPatternEInvoice>(entity =>
            //{
            //    entity.HasIndex(p => new { p.VFkey }).IsUnique();
            //    entity.HasIndex(p => new { p.TypeSupplierEInvoice });
            //    entity.HasIndex(p => new { p.ComId });
            //});
            //builder.Entity<SupplierEInvoice>(entity =>
            //{
            //    entity.HasMany(x => x.ManagerPatternEInvoices).WithOne(x => x.SupplierEInvoice).HasForeignKey(x => x.IdSupplierEInvoice);
            //    entity.HasIndex(p => new { p.TypeSupplierEInvoice, p.ComId }).IsUnique();
            //});



            //////////
            ///

            base.OnModelCreating(builder);

            // https://docs.microsoft.com/vi-vn/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key .OnDelete(DeleteBehavior.Cascade); 
        }

    }
}
