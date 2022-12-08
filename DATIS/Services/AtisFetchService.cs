using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DATIS.Models;

namespace DATIS.Services
{
    public class AtisFetchService
    {
        public string ApiBaseUrl { get; }
        private static readonly HttpClient _client = new(); // Should  replace with HTTPClientFactory one day...
        private static JsonSerializerOptions _jsonOptions;

        public AtisFetchService() : this(Constants.atisApiBaseUrl) { }

        public AtisFetchService(string apiBaseUrl)
        {
            ApiBaseUrl = apiBaseUrl;
            _client.BaseAddress = new Uri(apiBaseUrl);
            _jsonOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new AtisTypeEnumConverter()
                },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<Atis>> GetAllAsync()
        {
            try
            {
                return await _client.GetFromJsonAsync<List<Atis>>(Constants.allAirports, _jsonOptions);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class AtisTypeEnumConverter : JsonConverter<AtisType>
    {
        // CanConvert does not need to be implemented here since we only convert MyBoolEnum.
    
        public override AtisType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string enumValue = reader.GetString();
            switch (enumValue)
            {
                case "combined":
                    return AtisType.Combined;
                case "dep":
                    return AtisType.Departure;
                case "arr":
                    return AtisType.Arrival;
                default:
                    throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, AtisType value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case AtisType.Combined:
                    writer.WriteStringValue("combined");
                    break;
                case AtisType.Departure:
                    writer.WriteStringValue("dep");
                    break;
                case AtisType.Arrival:
                    writer.WriteStringValue("arr");
                    break;
                default:
                    throw new JsonException();
            }
        }
    }
}
