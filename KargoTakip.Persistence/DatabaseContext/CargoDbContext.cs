using KargoTakip.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KargoTakip.Persistence.DatabaseContext
{
    public class CargoDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public CargoDbContext(DbContextOptions<CargoDbContext> options) : base(options)
        {
        }

        public DbSet<Cargo> Cargos { get; set; }
        public DbSet<CargoMovement> CargoMovements { get; set; }
        public DbSet<CargoSender> CargoSenders { get; set; }
        public DbSet<CargoReceiver> CargoReceivers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cargo>()
                .HasMany(c => c.Movements)
                .WithOne(m => m.Cargo)
                .HasForeignKey(m => m.CargoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.Sender)
                .WithMany()
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cargo>()
                .HasOne(c => c.Receiver)
                .WithMany()
                .HasForeignKey(c => c.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedDate = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
