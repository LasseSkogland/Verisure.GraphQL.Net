using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Verisure.GraphQL.Data {
    public partial class LoginResponse {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
        [JsonPropertyName("accessTokenMaxAgeSeconds")]
        public int AccessTokenMaxAgeSeconds { get; set; }
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("refreshTokenMaxAgeSeconds")]
        public int RefreshTokenMaxAgeSeconds { get; set; }

        public DateTime RequestSent { get; set; }

        public DateTime AccessTokenExpire => RequestSent.AddSeconds(AccessTokenMaxAgeSeconds);
        public DateTime RefreshTokenExpire => RequestSent.AddSeconds(RefreshTokenMaxAgeSeconds);
    }
}
