using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace OrderAPI
{
    public static class ConfigureServices
    {
        public static IHttpClientBuilder AddCircuitBreakerPolicy(this IHttpClientBuilder builder)
        {
            return builder.AddPolicyHandler(GetPolicy());
        }
        private static IAsyncPolicy<HttpResponseMessage> GetPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(reponse => reponse.StatusCode == HttpStatusCode.InternalServerError)
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));
        }
    }
}