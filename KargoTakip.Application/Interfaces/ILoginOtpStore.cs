namespace KargoTakip.Application.Interfaces
{
    public class LoginOtpData
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public interface ILoginOtpStore
    {
        Task SetAsync(string loginToken, string userId, string email, string code, TimeSpan expiry, CancellationToken cancellationToken = default);
        Task<LoginOtpData?> GetAndRemoveAsync(string loginToken, CancellationToken cancellationToken = default);
    }
}
