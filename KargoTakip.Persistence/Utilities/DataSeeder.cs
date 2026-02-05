using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KargoTakip.Domain.Entities;
using KargoTakip.Domain.Enums;
using KargoTakip.Persistence.DatabaseContext;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace KargoTakip.Persistence.Utilities
{
    public class DataSeeder
    {
        private readonly CargoDbContext _context;
        private readonly string _connectionString;
        private readonly Random _rnd = new Random();

        public DataSeeder(CargoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task SeedAsync(int totalCargos = 100_000, int movementsPerCargo = 3)
        {
            if (totalCargos <= 0) return;

            Console.WriteLine($"🔄 Starting bulk seeding of {totalCargos:N0} cargos with {movementsPerCargo} movements each...");

            var sw = Stopwatch.StartNew();

            await SeedCargoSendersAndReceivers();

            var senderIds = await _context.CargoSenders
                .AsNoTracking()
                .Select(s => s.Id)
                .ToListAsync();

            var receiverIds = await _context.CargoReceivers
                .AsNoTracking()
                .Select(r => r.Id)
                .ToListAsync();

            if (!senderIds.Any() || !receiverIds.Any())
            {
                Console.WriteLine("❌ No senders or receivers found.");
                return;
            }

            await BulkInsertCargos(totalCargos, senderIds, receiverIds);

            await BulkInsertCargoMovements(movementsPerCargo);

            sw.Stop();
            Console.WriteLine($"✅ Seeding completed successfully in {sw.Elapsed.TotalSeconds:F2} seconds!");
        }

        private async Task SeedCargoSendersAndReceivers()
        {
            Console.WriteLine("📤 Seeding senders and receivers...");

            int poolSize = 5000;
            var existingSenders = await _context.CargoSenders.CountAsync();
            var existingReceivers = await _context.CargoReceivers.CountAsync();

            if (existingSenders > 0 && existingReceivers > 0)
            {
                Console.WriteLine($"⏭️  Senders ({existingSenders}) and receivers ({existingReceivers}) already exist. Skipping...");
                return;
            }

            var senders = new List<CargoSender>(poolSize);
            var receivers = new List<CargoReceiver>(poolSize);

            for (int i = 0; i < poolSize; i++)
            {
                senders.Add(new CargoSender
                {
                    FullName = $"Sender {i}",
                    FullAddress = $"Sender Address {i}, City {i % 100}",
                    PhoneNumber = $"5{_rnd.Next(100000000, 999999999)}"
                });

                receivers.Add(new CargoReceiver
                {
                    Name = $"ReceiverName{i}",
                    Surname = $"ReceiverSurname{i}",
                    FullAddress = $"Receiver Address {i}, City {i % 100}",
                    PhoneNumber = $"5{_rnd.Next(100000000, 999999999)}",
                    Email = $"receiver{i}@example.com"
                });
            }

            await _context.CargoSenders.AddRangeAsync(senders);
            await _context.CargoReceivers.AddRangeAsync(receivers);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Added {poolSize} senders and {poolSize} receivers");
        }

        private async Task BulkInsertCargos(int totalCargos, List<string> senderIds, List<string> receiverIds)
        {
            Console.WriteLine($"📦 Bulk inserting {totalCargos:N0} cargos...");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var bulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "Cargos",
                    BatchSize = 10000,
                    BulkCopyTimeout = 0
                })
                {
                    var dataTable = new DataTable();
                    dataTable.Columns.Add("Id", typeof(string));
                    dataTable.Columns.Add("TrackingNumber", typeof(string));
                    dataTable.Columns.Add("SenderId", typeof(string));
                    dataTable.Columns.Add("ReceiverId", typeof(string));
                    dataTable.Columns.Add("Status", typeof(int));
                    dataTable.Columns.Add("CreatedDate", typeof(DateTime));
                    dataTable.Columns.Add("UpdatedDate", typeof(DateTime));
                    dataTable.Columns.Add("IsDeleted", typeof(bool));

                    bulkCopy.ColumnMappings.Add("Id", "Id");
                    bulkCopy.ColumnMappings.Add("TrackingNumber", "TrackingNumber");
                    bulkCopy.ColumnMappings.Add("SenderId", "SenderId");
                    bulkCopy.ColumnMappings.Add("ReceiverId", "ReceiverId");
                    bulkCopy.ColumnMappings.Add("Status", "Status");
                    bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate");
                    bulkCopy.ColumnMappings.Add("UpdatedDate", "UpdatedDate");
                    bulkCopy.ColumnMappings.Add("IsDeleted", "IsDeleted");

                    var startDate = DateTime.UtcNow.AddYears(-1);

                    for (int i = 0; i < totalCargos; i++)
                    {
                        var createdDate = startDate.AddSeconds(_rnd.Next(0, (int)TimeSpan.FromDays(365).TotalSeconds));

                        dataTable.Rows.Add(
                            Guid.NewGuid().ToString(),
                            $"TRK-{DateTime.UtcNow:yyyyMMddHHmmss}-{i:D8}",
                            senderIds[_rnd.Next(senderIds.Count)],
                            receiverIds[_rnd.Next(receiverIds.Count)],
                            (int)CargoStatus.InTransit,
                            createdDate,
                            createdDate,
                            false
                        );

                        if ((i + 1) % 10000 == 0)
                        {
                            Console.WriteLine($"  📍 Prepared {i + 1:N0}/{totalCargos:N0} rows...");
                        }
                    }

                    await bulkCopy.WriteToServerAsync(dataTable);
                    Console.WriteLine($"✅ Cargos bulk inserted successfully");
                }
            }
        }

        private async Task BulkInsertCargoMovements(int movementsPerCargo)
        {
            Console.WriteLine($"📍 Bulk inserting cargo movements ({movementsPerCargo} per cargo)...");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand("SELECT Id, CreatedDate FROM Cargos WHERE IsDeleted = 0", connection))
                {
                    cmd.CommandTimeout = 0;
                    var cargoIds = new List<(string Id, DateTime CreatedDate)>();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            cargoIds.Add((reader.GetString(0), reader.GetDateTime(1)));
                        }
                    }

                    Console.WriteLine($"  Found {cargoIds.Count:N0} cargos for movements");

                    using (var bulkCopy = new SqlBulkCopy(connection)
                    {
                        DestinationTableName = "CargoMovements",
                        BatchSize = 10000,
                        BulkCopyTimeout = 0
                    })
                    {
                        var dataTable = new DataTable();
                        dataTable.Columns.Add("Id", typeof(string));
                        dataTable.Columns.Add("CargoId", typeof(string));
                        dataTable.Columns.Add("NewStatus", typeof(int));
                        dataTable.Columns.Add("Location", typeof(string));
                        dataTable.Columns.Add("Description", typeof(string));
                        dataTable.Columns.Add("CreatedDate", typeof(DateTime));
                        dataTable.Columns.Add("UpdatedDate", typeof(DateTime));
                        dataTable.Columns.Add("IsDeleted", typeof(bool));

                        bulkCopy.ColumnMappings.Add("Id", "Id");
                        bulkCopy.ColumnMappings.Add("CargoId", "CargoId");
                        bulkCopy.ColumnMappings.Add("NewStatus", "NewStatus");
                        bulkCopy.ColumnMappings.Add("Location", "Location");
                        bulkCopy.ColumnMappings.Add("Description", "Description");
                        bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate");
                        bulkCopy.ColumnMappings.Add("UpdatedDate", "UpdatedDate");
                        bulkCopy.ColumnMappings.Add("IsDeleted", "IsDeleted");

                        int totalRows = 0;
                        int cargoIndex = 0;

                        foreach (var (cargoId, cargoCreatedDate) in cargoIds)
                        {
                            for (int m = 0; m < movementsPerCargo; m++)
                            {
                                var movementDate = cargoCreatedDate.AddMinutes(m + 1);

                                dataTable.Rows.Add(
                                    Guid.NewGuid().ToString(),
                                    cargoId,
                                    (int)CargoStatus.InTransit,
                                    $"Location {_rnd.Next(1, 1000)}",
                                    $"Movement {m + 1} for cargo",
                                    movementDate,
                                    movementDate,
                                    false
                                );

                                totalRows++;
                            }

                            cargoIndex++;
                            if (cargoIndex % 5000 == 0 || cargoIndex == cargoIds.Count)
                            {
                                Console.WriteLine($"  📍 Prepared movements for {cargoIndex:N0}/{cargoIds.Count:N0} cargos ({totalRows:N0} movements)...");
                            }
                        }

                        await bulkCopy.WriteToServerAsync(dataTable);
                        Console.WriteLine($"✅ CargoMovements bulk inserted successfully ({totalRows:N0} total)");
                    }
                }
            }
        }

        public async Task ClearAllCargoDataAsync()
        {
            Console.WriteLine("🗑️  Clearing all cargo data...");

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM CargoMovements");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Cargos");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM CargoReceivers");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM CargoSenders");

            Console.WriteLine("✅ All cargo data cleared");
        }
    }
}

