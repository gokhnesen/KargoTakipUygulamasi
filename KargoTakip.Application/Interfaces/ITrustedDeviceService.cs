namespace KargoTakip.Application.Interfaces
{
    public interface ITrustedDeviceService
    {
        void SetTrustedDevice(string userId);
        string? GetTrustedUserIdIfValid();
    }
}
