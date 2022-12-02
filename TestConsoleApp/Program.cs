using System.Net.Http.Json;

public class TestConsoleApp
{ 
    public static class Constants
    {
        internal const string atisApiBaseUrl = "https://datis.clowd.io/api/";
        internal const string allAirports = "all";
    }

    public class Atis
    {
        public string Airport { get; set; }
        public string Type { get; set; }
        public string Datis { get; set; }
    }

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


    static async Task Main(string[] args)
    {
        var _atisFetcher = new AtisFetcher();
        List<Atis> _lastAtis = await _atisFetcher.GetAllAsync();
        foreach (var atis in _lastAtis)
        {
            Console.WriteLine(atis.Airport);
        }
    }
}