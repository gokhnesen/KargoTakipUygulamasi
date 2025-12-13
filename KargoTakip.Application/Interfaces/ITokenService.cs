using KargoTakip.Domain.Entities;
using System.Collections.Generic;

namespace KargoTakip.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);
        string GenerateRefreshToken();
    }
}