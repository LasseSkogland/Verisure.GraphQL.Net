using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Verisure.GraphQL.Data;

namespace Verisure.GraphQL {
    public class VerisureRetryHandler : DelegatingHandler {
        private readonly VerisureClient client;

        public VerisureRetryHandler(HttpClientHandler innerHandler, VerisureClient client) : base(innerHandler) {
            this.client = client;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            HttpResponseMessage response = null;
            for (int i = 0; i < 5; i++) {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode) {
                    return response;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized) {
                    if (client.cookieContainer.GetCookies(client.ApiUrl).Any(x => x.Name == "vs-refresh")) {
                        await client.RefreshTokenAsync();
                        response = await base.SendAsync(request, cancellationToken);
                        if (response.IsSuccessStatusCode) {
                            return response;
                        }
                    }
                    else {
                        throw new HttpRequestException("No RefreshToken found, unable to reauthenticate. Have you logged in?");
                    }
                }
            }
            return response;
        }
    }
}
