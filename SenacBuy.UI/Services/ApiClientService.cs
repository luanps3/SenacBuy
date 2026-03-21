using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace SenacBuy.UI.Services
{
    public static class ApiClientService
    {
        public const string ApiBaseUrl = "http://localhost:5156";

        private static readonly HttpClient _httpClient = CriarHttpClient();

        public static HttpClient Cliente => _httpClient;


        private static HttpClient CriarHttpClient()
        {
            var cliente = new HttpClient();
            cliente.BaseAddress = new Uri(ApiBaseUrl);

            // Informamos a API que esperamos resposta JSON
            cliente.DefaultRequestHeaders.Accept.Clear();
            cliente.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            cliente.Timeout = TimeSpan.FromSeconds(30); // Define um timeout de 30 segundos

            return cliente;
        }


    }
}
