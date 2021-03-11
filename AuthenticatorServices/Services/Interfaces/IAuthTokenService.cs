using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AuthenticatorServices.Models;
using IdentityModel.Client;

namespace AuthenticatorServices.Services.Interfaces
{
    public interface IAuthTokenService
    {
        Task<TokenResponse> GetAccessToken(
            HttpClient httpClient,
            AuthorizationSettingsModel.AuthorizationSettingModel setting,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}