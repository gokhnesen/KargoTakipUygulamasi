using KargoTakip.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            modelBuilder.Entity<Cargo>(entity =>
            {
                entity.Property(c => c.TrackingNumber).HasMaxLength(64);

                entity.HasIndex(c => new { c.CreatedDate, c.Id })
                      .HasDatabaseName("IX_Cargos_CreatedDate_Id")
                      .HasFilter("[IsDeleted] = 0");

                entity.HasIndex(c => c.TrackingNumber)
                      .HasDatabaseName("IX_Cargos_TrackingNumber")
                      .HasFilter("[IsDeleted] = 0");

                entity.HasIndex(c => new { c.Status, c.CreatedDate, c.Id })
                      .HasDatabaseName("IX_Cargos_Status_CreatedDate_Id")
                      .HasFilter("[IsDeleted] = 0");

                entity.HasIndex(c => c.SenderId)
                      .HasDatabaseName("IX_Cargos_SenderId")
                      .HasFilter("[IsDeleted] = 0");

                entity.HasIndex(c => c.ReceiverId)
                      .HasDatabaseName("IX_Cargos_ReceiverId")
                      .HasFilter("[IsDeleted] = 0");

                entity.HasIndex(c => c.Status)
                      .HasDatabaseName("IX_Cargos_Status")
                      .HasFilter("[IsDeleted] = 0");

                entity.Property(c => c.LastLocation).HasMaxLength(256);
            });

            modelBuilder.Entity<CargoMovement>(entity =>
            {
                entity.HasIndex(m => new { m.CargoId, m.CreatedDate, m.Id })
                        .HasDatabaseName("IX_CargoMovements_CargoId_CreatedDate_Id")
                        .HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<CargoReceiver>(entity =>
            {
                entity.Property(r => r.Email).HasMaxLength(256);
                entity.HasIndex(r => r.Email)
                      .HasDatabaseName("IX_CargoReceivers_Email")
                      .HasFilter("[IsDeleted] = 0");

                entity.Property(r => r.Name).HasMaxLength(128);
                entity.Property(r => r.Surname).HasMaxLength(128);

                entity.HasIndex(r => new { r.Name, r.Surname })
                      .HasDatabaseName("IX_CargoReceivers_Name_Surname")
                      .HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<CargoSender>(entity =>
            {
                entity.Property(s => s.FullName).HasMaxLength(256);
                entity.HasIndex(s => s.FullName)
                      .HasDatabaseName("IX_CargoSenders_FullName")
                      .HasFilter("[IsDeleted] = 0");
            });

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
