using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AuthenticatorServices.Exceptions;
using AuthenticatorServices.Models;
using AuthenticatorServices.Services.Interfaces;
using IdentityModel.Client;

namespace AuthenticatorServices.Services
{
    internal class AuthTokenService : IAuthTokenService
    {
        public async Task<TokenResponse> GetAccessToken(
            HttpClient httpClient,
            AuthorizationSettingsModel.AuthorizationSettingModel setting,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(setting.UserName) && !string.IsNullOrEmpty(setting.Password))
                return await GetAccessToken(setting.Scopes, httpClient, setting.AuthorizationUrl, setting.ClientId,
                    setting.UserName, setting.Password, cancellationToken);

            return await GetAccessToken(setting.Scopes, httpClient, setting.AuthorizationUrl, setting.ClientId, setting.ClientSecret, cancellationToken);
        }

        private async Task<TokenResponse> GetAccessToken(
            string scopes,
            HttpClient httpClient,
            string tokenUrl,
            string clientId,
            string secret,
            CancellationToken token)
        {
            var client = httpClient;
            var request = new ClientCredentialsTokenRequest
            {
                Scope = scopes, Address = tokenUrl, ClientId = clientId, ClientSecret = secret
            };
            var cancellationToken = token;
            var response = await client.RequestClientCredentialsTokenAsync(request, cancellationToken);
            return !response.IsError ? response : throw new OAuth2ErrorException(response.Error, response);
        }

        private async Task<TokenResponse> GetAccessToken(
            string scopes,
            HttpClient httpClient,
            string tokenUrl,
            string clientId,
            string userName,
            string password,
            CancellationToken token)
        {
            var client = httpClient;
            var request = new PasswordTokenRequest
            {
                Scope = scopes,
                Address = tokenUrl,
                ClientId = clientId,
                UserName = userName,
                Password = password
            };
            var cancellationToken = token;
            var response = await client.RequestPasswordTokenAsync(request, cancellationToken);
            return !response.IsError ? response : throw new OAuth2ErrorException(response.Error, response);
        }
    }
}