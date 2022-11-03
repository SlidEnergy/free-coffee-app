using Newtonsoft.Json;
using PointsChecker.Api;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PointsChecker
{
    public class ProductsService : IProductsService
    {
        HttpClient httpClient;
        Configuration configuration;

        public ProductsService(Configuration config)
        {
            httpClient = new HttpClient();
            configuration = config;
            httpClient.BaseAddress = new Uri(config.BaseUrl);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("x-api-token", config.ApiToken);
        }

        public async Task<List<Product>> GetFreeProductsAsync(string userId)
        {
            var result =  await httpClient.PostAsync($"{configuration.CheckPointsEndPoint}?user_id={userId}", null);

            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<Product>>(json);
            }

            if(result.StatusCode == HttpStatusCode.Forbidden)
            {
                var json = await result.Content.ReadAsStringAsync();

                var error = JsonConvert.DeserializeObject<ForbiddenErrorResponse>(json);

                throw new UnauthorizedAccessException(error.Error);
            }

            if (result.StatusCode == (HttpStatusCode)422)
            {
                var json = await result.Content.ReadAsStringAsync();

                var error = JsonConvert.DeserializeObject<UnprocessableErrorResponse>(json);

                throw new ApiErrorException(error.Message);
            }

            throw new UnhandledApiErrorException("Unhandled http error " + result.ReasonPhrase);
        }

        public async Task<string> OrderProductAsync(Product product, string userId)
        {
            var result = await httpClient.PostAsync($"{configuration.ConsumeEndpoint}?user_id={userId}&campaign_product_id={product.Id}", null);

            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<SuccessCunsumeResponse>(json).Message;
            }

            if (result.StatusCode == HttpStatusCode.Forbidden)
            {
                var json = await result.Content.ReadAsStringAsync();

                var error = JsonConvert.DeserializeObject<ForbiddenErrorResponse>(json);

                throw new UnauthorizedAccessException(error.Error);
            }

            if (result.StatusCode == (HttpStatusCode)422)
            {
                var json = await result.Content.ReadAsStringAsync();

                var error = JsonConvert.DeserializeObject<UnprocessableErrorResponse>(json);

                throw new ApiErrorException(error.Message);
            }

            throw new UnhandledApiErrorException("Unhandled http error " + result.ReasonPhrase);
        }
    }
}
