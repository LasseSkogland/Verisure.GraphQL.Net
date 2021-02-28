using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Verisure.GraphQL.Data;

namespace Verisure.GraphQL {
    public class VerisureClient : IDisposable {

        private bool _disposed = false;

        internal readonly Uri ApiUrl = new Uri("https://m-api02.verisure.com");
        internal HttpClient httpClient;
        internal CookieContainer cookieContainer;

        protected string username;
        protected SecureString password;

        public VerisureClient(string username, string password) : this(username, password.ToSecureString()) { }
        public VerisureClient(string username, SecureString password) {
            this.username = username;
            this.password = password;

            cookieContainer = new CookieContainer();
            httpClient = new HttpClient(new VerisureRetryHandler(new HttpClientHandler() { CookieContainer = cookieContainer }, this)) {
                BaseAddress = ApiUrl
            };
            httpClient.DefaultRequestHeaders.Add("APPLICATION_ID", "MyPages_via_GraphQL");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:15.0) Gecko/20100101 Firefox/15");
        }

        public void Dispose() {
            if (_disposed) return;
            httpClient?.Dispose();
            password?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public async Task<LoginResponse> LoginAsync() {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/auth/login");
            req.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password.ToClearTextString()}"))}");
            req.Headers.Add("APPLICATION_ID", "MyPages_Login");
            var requestTime = DateTime.Now;
            var res = await httpClient.SendAsync(req);

            var contentText = await res.Content.ReadAsStringAsync();
            var content = JsonSerializer.Deserialize<LoginResponse>(contentText);

            content.RequestSent = requestTime;
            return content;
        }

        public async Task<LoginResponse> RefreshTokenAsync() {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/auth/token");
            req.Headers.Add("APPLICATION_ID", "MyPages_Login");

            var requestTime = DateTime.Now;
            var res = await httpClient.SendAsync(req);

            var contentText = await res.Content.ReadAsStringAsync();
            var content = JsonSerializer.Deserialize<LoginResponse>(contentText);

            content.RequestSent = requestTime;
            return content;
        }

        public async Task<dynamic> QueryAsync(GraphQLQuery query) {
            return await QueryAsync(new[] { query });
        }

        public async Task<IDictionary<string, dynamic>> QueryAsync(GraphQLQuery[] queries) {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/graphql") {
                Content = new StringContent(JsonSerializer.Serialize(queries.Select(x => x.ToDictionary())))
            };
            var res = await httpClient.SendAsync(req);

            var contentText = await res.Content.ReadAsStringAsync();
            var content = JsonSerializer.Deserialize<JsonElement>(contentText);

            var dict = content.CreateDictionary();
            return dict;
        }
    }
}
