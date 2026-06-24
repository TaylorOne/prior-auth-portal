using PriorAuthApi.DTOs;
using System.Text.Json;
using System.Net.Http.Json;
using FluentAssertions;
using System.Net;

namespace PriorAuthApi.Tests
{
    public class PriorAuthEndpointTests : IClassFixture<WebAppFactory>
    {
        private readonly HttpClient _client;

        public PriorAuthEndpointTests(WebAppFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostPriorAuth_ValidRequest_Returns201()
        {
            var dto = new SubmitPriorAuthDto(
                Priority: "routine",
                Code: new CodeableConceptDto("HCPCS", "J0135", "Adalimumab injection"),
                PatientId: 1,
                ReasonCode: ["M06.9"],
                ClinicalData: new Dictionary<string, JsonElement>
                {
                    ["priorDMARDTrial"] = JsonDocument.Parse("true").RootElement,
                    ["dmardName"] = JsonDocument.Parse("\"Methotrexate\"").RootElement,
                    ["dmardDurationWeeks"] = JsonDocument.Parse("16").RootElement,
                    ["notes"] = JsonDocument.Parse("\"Patient tolerated poorly.\"").RootElement
                },
                MedicationRequest: null
            );

            var response = await _client.PostAsJsonAsync("/priorauth", dto);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task PostPriorAuth_SpecialtyMismatch_Returns400WithProblemDetails()
        {
            // Test practitioner has Specialty.Rheumatology; this rule requires Specialty.Oncology
            var dto = new SubmitPriorAuthDto(
                Priority: "routine",
                Code: new CodeableConceptDto("CPT", "81162", "Genetic Testing (BRCA)"),
                PatientId: 1,
                ReasonCode: ["Z15.01"],
                ClinicalData: new Dictionary<string, JsonElement>(),
                MedicationRequest: null
            );

            var response = await _client.PostAsJsonAsync("/priorauth", dto);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var problem = JsonDocument.Parse(body).RootElement;
            problem.GetProperty("detail").GetString().Should()
                .Contain("Oncology").And.Contain("Rheumatology");
        }
    }
}