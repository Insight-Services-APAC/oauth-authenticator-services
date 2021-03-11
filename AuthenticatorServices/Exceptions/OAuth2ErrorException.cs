using System;
using IdentityModel.Client;

namespace AuthenticatorServices.Exceptions
{
    public class OAuth2ErrorException : Exception
    {
        public TokenResponse TokenResponse { get; }

        public OAuth2ErrorException(string message, TokenResponse response)
            : base(message)
            => this.TokenResponse = response;
    }
}