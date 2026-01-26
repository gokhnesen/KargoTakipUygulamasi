using KargoTakip.Domain.Enums;

namespace KargoTakip.Application.Services
{
    public class CargoStatusHelper
    {
        public string GetStatusMessage(CargoStatus status)
        {
            return status switch
            {
                CargoStatus.Received => "teslim alindi",
                CargoStatus.InTransit => "yolda",
                CargoStatus.OutForDelivery => "dagitimda",
                CargoStatus.Delivered => "teslim edildi",
                CargoStatus.Undelivered => "teslim edilemedi",
                _ => "guncellendi"
            };
        }
    }
}
