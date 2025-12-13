using KargoTakip.Application.Exceptions;
using KargoTakip.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace KargoTakip.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterCommandResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterCommandHandler> _logger;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<RegisterCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<RegisterCommandResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Yeni kullanıcı kaydı başlatılıyor: {Email}", request.Email);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Kullanıcı zaten mevcut: {Email}", request.Email);
                throw new AuthenticationException("Bu email adresi zaten kullanılıyor.");
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Kullanıcı kaydı başarısız: {Errors}", errors);
                throw new AuthenticationException($"Kullanıcı kaydı başarısız: {errors}");
            }

            _logger.LogInformation("Kullanıcı başarıyla kaydedildi: {Email}", request.Email);

            return new RegisterCommandResponse
            {
                Success = true,
                Message = "Kullanıcı başarıyla kaydedildi.",
                UserId = user.Id.ToString()
            };
        }
    }
}