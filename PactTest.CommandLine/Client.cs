using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace PactTest.CommandLine
{
    public class Client : IClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _httpClient;
        
        public Client(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<IReadOnlyCollection<Order>> GetAllAsync()
        {
            using var response = await _httpClient.GetAsync("/order");
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            
            return await JsonSerializer.DeserializeAsync<List<Order>>(responseStream, JsonOptions);
        }
        
        public async Task<Order> GetByIdAsync(int id)
        {
            using var response = await _httpClient.GetAsync($"/order/{id}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Invalid request: {response.StatusCode} {response.ReasonPhrase}");
            }
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            
            return await JsonSerializer.DeserializeAsync<Order>(responseStream, JsonOptions);
        }     
        
        public async Task<Order> AddAsync(OrderAdd model)
        {
            var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(model));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            
            using var response = await _httpClient.PostAsync("/order", content);
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            
            return await JsonSerializer.DeserializeAsync<Order>(responseStream, JsonOptions);
        }  
        
        public async Task<Order> UpdateAsync(int id, OrderUpdate model)
        {
            var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(model));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            using var response = await _httpClient.PutAsync($"/order/{id}", content);
            if ((int)response.StatusCode < 300 || (int)response.StatusCode >= 400)
            {
                throw new Exception($"Invalid request: {response.StatusCode} {response.ReasonPhrase}");
            }

            return await GetByIdAsync(id);
        }  
        
        public async Task<bool> DeleteAsync(int id)
        {
            using var response = await _httpClient.DeleteAsync($"/order/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}