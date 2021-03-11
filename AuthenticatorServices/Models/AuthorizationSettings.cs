using System.Collections.Generic;

namespace AuthenticatorServices.Models
{
    public class AuthorizationSettingsModel
    {
        public class AuthorizationSettingModel
        {
            public string Endpoint { get; set; }

            public string AuthorizationUrl { get; set; }

            public string Scopes { get; set; }

            public string ClientId { get; set; }

            public string ClientSecret { get; set; }

            public string UserName { get; set; }

            public string Password { get; set; }
        }
        public List<AuthorizationSettingModel> Settings { get; set; }
    }
}