using EdenRequest.Api.DTO;
using EdenRequest.Api.options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EdenRequest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly DigitransitOptions _options;

        public AddressController(HttpClient httpClient, IOptions<DigitransitOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup([FromQuery] string query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
                return Ok(Enumerable.Empty<FinnishAddressResultDto>());

            var requestUrl = $"https://api.digitransit.fi/geocoding/v1/autocomplete?text={Uri.EscapeDataString(query)}&boundary.country=FIN&lang=fi";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            if (!string.IsNullOrEmpty(_options.ApiKey))
            {
                request.Headers.Add("digitransit-subscription-key", _options.ApiKey);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Address lookup failed");

            // Fixed: Called .ReadAsStreamAsync on response.Content instead of response directly
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var results = new List<FinnishAddressResultDto>();
            if (jsonDoc.RootElement.TryGetProperty("features", out var features))
            {
                foreach (var feat in features.EnumerateArray())
                {
                    var props = feat.GetProperty("properties");
                    var coords = feat.GetProperty("geometry").GetProperty("coordinates");

                    results.Add(new FinnishAddressResultDto
                    {
                        Label = props.GetProperty("label").GetString() ?? "",
                        Street = props.TryGetProperty("street", out var s) ? s.GetString() ?? "" : "",
                        HouseNumber = props.TryGetProperty("housenumber", out var h) ? h.GetString() ?? "" : "",
                        PostalCode = props.TryGetProperty("postalcode", out var p) ? p.GetString() ?? "" : "",
                        City = props.TryGetProperty("locality", out var c) ? c.GetString() ?? "" : "",
                        Latitude = coords[1].GetDouble(),
                        Longitude = coords[0].GetDouble()
                    });
                }
            }

            return Ok(results);
        }
    }
    }
