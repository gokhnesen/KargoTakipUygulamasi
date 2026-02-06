using KargoTakip.Application.Dtos;
using KargoTakip.Application.Features.Cargo.Queries.GetAll;
using KargoTakip.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace KargoTakip.Application.Mappings
{
    /// <summary>
    /// Cargo entity -> DTO mapping; "Bilinmiyor" gibi display mantığı burada tutulur, handler'da değil.
    /// </summary>
    public static class CargoMapping
    {
        private const string UnknownDisplay = "Bilinmiyor";

        /// <summary>Liste cevabi: sadece LastLocation doner, Movements donmez.</summary>
        public static GetAllCargosQueryResponse ToGetAllCargosQueryResponse(this Cargo c)
        {
            return new GetAllCargosQueryResponse
            {
                Id = c.Id,
                TrackingNumber = c.TrackingNumber,
                SenderName = c.Sender?.FullName ?? UnknownDisplay,
                SenderAddress = c.Sender?.FullAddress ?? UnknownDisplay,
                SenderPhoneNumber = c.Sender?.PhoneNumber ?? UnknownDisplay,
                ReceiverName = c.Receiver?.Name ?? UnknownDisplay,
                ReceiverSurname = c.Receiver?.Surname ?? string.Empty,
                ReceiverAddress = c.Receiver?.FullAddress ?? UnknownDisplay,
                ReceiverPhoneNumber = c.Receiver?.PhoneNumber ?? UnknownDisplay,
                ReceiverEmail = c.Receiver?.Email ?? UnknownDisplay,
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                UpdatedDate = c.UpdatedDate,
                LastLocation = c.LastLocation ?? c.Movements?.OrderByDescending(m => m.CreatedDate).FirstOrDefault()?.Location
            };
        }

        public static CargoMovementDto ToCargoMovementDto(this CargoMovement m)
        {
            return new CargoMovementDto
            {
                Id = m.Id,
                NewStatus = m.NewStatus,
                Location = m.Location,
                Description = m.Description,
                CreatedDate = m.CreatedDate
            };
        }
    }
}
