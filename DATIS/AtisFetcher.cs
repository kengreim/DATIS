using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DATIS.Models;

namespace DATIS
{
    public class AtisFetcher
    {
        public string ApiBaseUrl { get; }
        private static readonly HttpClient _client = new(); // Should  replace with HTTPClientFactory one day...

        public AtisFetcher() : this(Constants.atisApiBaseUrl) { }

        public AtisFetcher(string apiBaseUrl)
        {
            ApiBaseUrl = apiBaseUrl;
            _client.BaseAddress = new Uri(apiBaseUrl);
        }

        public async Task<List<Atis>> GetAllAsync()
        {
            try
            {
                return await _client.GetFromJsonAsync<List<Atis>>(Constants.allAirports);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
