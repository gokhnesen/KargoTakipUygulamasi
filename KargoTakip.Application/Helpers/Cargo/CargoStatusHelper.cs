using KargoTakip.Domain.Enums;

namespace KargoTakip.Application.Helpers.Cargo
{
    public static class CargoStatusHelper
    {
        public static string GetStatusMessage(CargoStatus status)
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
