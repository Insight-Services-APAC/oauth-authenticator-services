using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AuthenticatorServices.Models;
using AuthenticatorServices.Services.Interfaces;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthenticatorServices
{
    public class AuthenticatorEndpointHandler : DelegatingHandler
    {
        private readonly IAuthTokenService _authTokenService;
        private readonly AuthorizationSettingsModel _authorizationSettings;
        private readonly IMemoryCache _cacheService;
        private readonly ILogger<AuthenticatorEndpointHandler> _logger;
        private readonly HttpClient _httpClient;
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        public AuthenticatorEndpointHandler(
            HttpClient httpClient,
            IAuthTokenService authTokenService,
            IOptions<AuthorizationSettingsModel> appSettings,
            IMemoryCache memoryCache,
            ILogger<AuthenticatorEndpointHandler> logger)
        {
            _authTokenService = authTokenService;
            _authorizationSettings = appSettings.Value;
            _httpClient = httpClient;
            _cacheService = memoryCache;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                var utcNow = DateTimeOffset.UtcNow;
                var key = request.RequestUri.GetLeftPart(UriPartial.Query);
                var setting = _authorizationSettings.Settings.FirstOrDefault(c => key.IndexOf(c.Endpoint, StringComparison.OrdinalIgnoreCase) == 0);

                if (setting == null)
                {
                    _logger.LogDebug("No auth setting found for {uri} using value of {key}", request.RequestUri, key);
                    return await base.SendAsync(request, cancellationToken);
                }

                _httpClient.BaseAddress = _httpClient.BaseAddress ?? new Uri(setting.Endpoint);

                if (!_cacheService.TryGetValue<string>(key, out var accessToken))
                {
                    var tokenResponse = await _authTokenService.GetAccessToken(_httpClient, setting, cancellationToken);

                    if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
                    {
                        _logger.LogDebug("HttpStatusCode.Unauthorized: Access token was null or invalid.");

                        return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                        {
                            Content = new StringContent("Access token was null or invalid")
                        };
                    }

                    accessToken = tokenResponse.AccessToken;
                    _logger.LogDebug("GetAccessToken: Access token {AccessToken}", accessToken);

                    _cacheService.Set(key, accessToken, utcNow.AddMinutes(30.0));
                }

                request.SetBearerToken(accessToken);

                var httpResponseMessage = await base.SendAsync(request, cancellationToken);

                if (httpResponseMessage.StatusCode != HttpStatusCode.Unauthorized)
                    return httpResponseMessage;

                await SemaphoreSlim.WaitAsync(cancellationToken);

                try
                {
                    var tokenResponse = await _authTokenService.GetAccessToken(_httpClient, setting, cancellationToken);
                    if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
                    {
                        return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                        {
                            Content = new StringContent("Refresh token was null or invalid")
                        };
                    }

                    accessToken = tokenResponse.AccessToken;
                    _cacheService.Set(key, accessToken, utcNow.AddMinutes(30.0));
                    request.SetBearerToken(accessToken);

                    _logger.LogDebug("Got new refreshed token, resending request");

                    return await base.SendAsync(request, cancellationToken);
                }
                catch
                {
                    _cacheService.Remove(key);
                    throw;
                }
                finally
                {
                    SemaphoreSlim.Release();
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Failed to get the access token. TaskCanceledException: {Message}, InnerException: {Message}", ex.Message, ex.InnerException?.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Exception occurred while getting access token: {Message}", ex.Message);
                throw;
            }
        }
    }
}