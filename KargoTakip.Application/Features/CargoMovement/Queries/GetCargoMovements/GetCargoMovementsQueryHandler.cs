using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.CargoMovement;
using KargoTakip.Domain.Entities;
using KargoTakip.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.CargoMovement.Queries.GetCargoMovements
{
    public class GetCargoMovementsQueryHandler : IRequestHandler<GetCargoMovementsQuery, GetCargoMovementsQueryResponse>
    {
        private readonly ICargoMovementRepository _movementRepository;
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly ILogger<GetCargoMovementsQueryHandler> _logger;

        public GetCargoMovementsQueryHandler(
            ICargoMovementRepository movementRepository,
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            ILogger<GetCargoMovementsQueryHandler> logger)
        {
            _movementRepository = movementRepository;
            _cargoRepository = cargoRepository;
            _logger = logger;
        }

        public async Task<GetCargoMovementsQueryResponse> Handle(GetCargoMovementsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kargo hareketleri getiriliyor. CargoId: {CargoId}", request.CargoId);

            var cargo = await _cargoRepository.GetValueByExpressionAsync(c => c.Id == request.CargoId && !c.IsDeleted, cancellationToken);
            if (cargo == null)
            {
                _logger.LogWarning("Kargo bulunamadi. CargoId: {CargoId}", request.CargoId);
                throw new NotFoundException("Kargo bulunamadi.");
            }

            var movements = await _movementRepository.GetMovementsByCargoIdAsync(request.CargoId, cancellationToken);

            var response = new GetCargoMovementsQueryResponse
            {
                CargoId = cargo.Id,
                TrackingNumber = cargo.TrackingNumber,
                Movements = movements.Select(m => new Domain.Entities.CargoMovement
                {
                    Id = m.Id,
                    NewStatus = m.NewStatus,
                    Location = m.Location,
                    Description = m.Description,
                    CreatedDate = m.CreatedDate
                }).ToList()
            };

            _logger.LogInformation("Kargo hareketleri getirildi. Hareket sayisi: {Count}", movements.Count);
            return response;
        }
    }
}