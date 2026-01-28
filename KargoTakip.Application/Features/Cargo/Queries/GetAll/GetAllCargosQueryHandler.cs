using KargoTakip.Application.Dtos;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.Cargo;
using KargoTakip.Application.Services;
using KargoTakip.Application.Specifications.CargoSpecifications;
using KargoTakip.Domain.Entities;
using KargoTakip.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Cargo.Queries.GetAll
{
    public class GetAllCargosQueryHandler : IRequestHandler<GetAllCargosQuery, GetAllCargosQueryResult>
    {
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly ILogger<GetAllCargosQueryHandler> _logger;
        private readonly CargoStatusHelper _statusHelper;

        public GetAllCargosQueryHandler(
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            ILogger<GetAllCargosQueryHandler> logger,
            CargoStatusHelper statusHelper)
        {
            _cargoRepository = cargoRepository;
            _logger = logger;
            _statusHelper = statusHelper;
        }

        public async Task<GetAllCargosQueryResult> Handle(GetAllCargosQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tum kargolar listeleniyor. Filtreler: Status={Status}, TrackingNumber={TrackingNumber}, Email={Email}, Page={Page}", 
                request.Status, request.TrackingNumber, request.ReceiverEmail, request.PageIndex);

            var filterParams = new CargoFilterParams
            {
                Status = request.GetCargoStatus(),
                TrackingNumber = request.TrackingNumber,
                SenderName = request.SenderName,
                ReceiverName = request.ReceiverName,
                ReceiverEmail = request.ReceiverEmail,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                OrderBy = request.OrderBy,
                IsDescending = request.IsDescending,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            var spec = new CargoFilterSpecification(filterParams);

            
            var cargos = await _cargoRepository.GetAllWithSpecAsync(spec);
            
            
            
            var countSpec = new CargoFilterSpecification(new CargoFilterParams
            {
                Status = request.GetCargoStatus(),
                TrackingNumber = request.TrackingNumber,
                SenderName = request.SenderName,
                ReceiverName = request.ReceiverName,
                ReceiverEmail = request.ReceiverEmail,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                PageSize = 0 
            });
            var totalCount = await _cargoRepository.CountAsync(countSpec);

            _logger.LogInformation("Veritabanindan {Count} adet kargo getirildi (Toplam: {TotalCount}).", cargos?.Count ?? 0, totalCount);


            if (cargos == null || !cargos.Any())
            {
                _logger.LogWarning("Hic kargo bulunamadi.");
                return new GetAllCargosQueryResult
                {
                    Items = new List<GetAllCargosQueryResponse>(),
                    TotalCount = 0,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize
                };
            }

            var response = cargos.Select(c => new GetAllCargosQueryResponse
            {
                Id = c.Id,
                TrackingNumber = c.TrackingNumber,
                SenderName = c.Sender?.FullName ?? "Bilinmiyor",
                SenderAddress = c.Sender?.FullAddress ?? "Bilinmiyor",
                SenderPhoneNumber = c.Sender?.PhoneNumber ?? "Bilinmiyor",
                ReceiverName = c.Receiver?.Name ?? "Bilinmiyor",
                ReceiverSurname = c.Receiver?.Surname ?? string.Empty,
                ReceiverAddress = c.Receiver?.FullAddress ?? "Bilinmiyor",
                ReceiverPhoneNumber = c.Receiver?.PhoneNumber ?? "Bilinmiyor",
                ReceiverEmail = c.Receiver?.Email ?? "Bilinmiyor",
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate,
                Movements = c.Movements?.OrderByDescending(m => m.CreatedDate).Select(m => new CargoMovementDto
                {
                    Id = m.Id,
                    NewStatus = m.NewStatus,
                    Location = m.Location,
                    Description = m.Description,
                    CreatedDate = m.CreatedDate
                }).ToList() ?? new List<CargoMovementDto>()
            }).ToList();

            _logger.LogInformation("{Count} adet kargo basariyla listelendi.", response.Count);

            return new GetAllCargosQueryResult
            {
                Items = response,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };
        }
    }
}