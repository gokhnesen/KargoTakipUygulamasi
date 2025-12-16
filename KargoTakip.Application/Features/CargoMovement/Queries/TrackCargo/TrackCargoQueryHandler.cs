using KargoTakip.Application.Exceptions;
using KargoTakip.Application.Interfaces;
using KargoTakip.Application.Interfaces.CargoMovement;
using KargoTakip.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.CargoMovement.Queries.TrackCargo
{
    public class TrackCargoQueryHandler : IRequestHandler<TrackCargoQuery, TrackCargoQueryResponse>
    {
        private readonly ICargoMovementRepository _movementRepository;
        private readonly IGenericRepository<Domain.Entities.Cargo> _cargoRepository;
        private readonly IGenericRepository<Domain.Entities.CargoSender> _senderRepository;
        private readonly IGenericRepository<Domain.Entities.CargoReceiver> _receiverRepository;
        private readonly ILogger<TrackCargoQueryHandler> _logger;

        public TrackCargoQueryHandler(
            ICargoMovementRepository movementRepository,
            IGenericRepository<Domain.Entities.Cargo> cargoRepository,
            IGenericRepository<Domain.Entities.CargoSender> senderRepository,
            IGenericRepository<Domain.Entities.CargoReceiver> receiverRepository,
            ILogger<TrackCargoQueryHandler> logger)
        {
            _movementRepository = movementRepository;
            _cargoRepository = cargoRepository;
            _senderRepository = senderRepository;
            _receiverRepository = receiverRepository;
            _logger = logger;
        }

        public async Task<TrackCargoQueryResponse> Handle(TrackCargoQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kargo takip sorgusu başlatıldı. TrackingNumber: {TrackingNumber}", request.TrackingNumber);

            var cargo = await _cargoRepository.GetValueByExpressionAsync(
                c => c.TrackingNumber == request.TrackingNumber && !c.IsDeleted);

            if (cargo == null)
            {
                _logger.LogWarning("Kargo bulunamadı. TrackingNumber: {TrackingNumber}", request.TrackingNumber);
                throw new NotFoundException("Takip numarası ile eşleşen kargo bulunamadı.");
            }

            var sender = await _senderRepository.GetValueByExpressionAsync(s => s.Id == cargo.SenderId);
            var receiver = await _receiverRepository.GetValueByExpressionAsync(r => r.Id == cargo.ReceiverId);
            var movements = await _movementRepository.GetMovementsByCargoIdAsync(cargo.Id);

            var latestMovement = movements.FirstOrDefault();

            var response = new TrackCargoQueryResponse
            {
                TrackingNumber = cargo.TrackingNumber,
                CurrentStatus = cargo.Status,
                CurrentStatusText = GetStatusText(cargo.Status),
                CurrentLocation = latestMovement?.Location ?? "Bilgi yok",
                Sender = sender != null ? new SenderInfo
                {
                    FullName = sender.FullName,
                    Address = sender.FullAddress,
                    PhoneNumber = sender.PhoneNumber
                } : null,
                Receiver = receiver != null ? new ReceiverInfo
                {
                    FullName = $"{receiver.Name} {receiver.Surname}",
                    Address = receiver.FullAddress,
                    PhoneNumber = receiver.PhoneNumber,
                    Email = receiver.Email
                } : null,
                Movements = movements.Select(m => new MovementInfo
                {
                    Status = m.NewStatus,
                    StatusText = GetStatusText(m.NewStatus),
                    Location = m.Location,
                    Description = m.Description,
                    Date = m.CreatedDate
                }).ToList()
            };

            _logger.LogInformation("Kargo takip bilgileri getirildi. Hareket sayısı: {Count}", movements.Count);
            return response;
        }

        private string GetStatusText(CargoStatus status)
        {
            return status switch
            {
                CargoStatus.Received => "Teslim Alındı",
                CargoStatus.InTransit => "Yolda",
                CargoStatus.OutForDelivery => "Dağıtımda",
                CargoStatus.Delivered => "Teslim Edildi",
                CargoStatus.Undelivered => "Teslim Edilemedi",
                _ => "Bilinmiyor"
            };
        }
    }
}