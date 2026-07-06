using PriorAuthApi.DTOs;
using PriorAuthApi.Services;
using PriorAuth.Contracts;
using PriorAuth.Data;
using PriorAuth.Data.Entities;
using System.Text.Json;
using System.Net.Http.Json;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;

namespace PriorAuthApi.Tests
{
    public class PriorAuthEndpointTests : IClassFixture<WebAppFactory>
    {
        private readonly WebAppFactory _factory;
        private readonly HttpClient _client;

        public PriorAuthEndpointTests(WebAppFactory factory)
        {
            _factory = factory;
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

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task PostPriorAuth_WritesOutboxMessageInSameTransaction()
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
                    ["notes"] = JsonDocument.Parse("\"Outbox test.\"").RootElement
                },
                MedicationRequest: null
            );

            var response = await _client.PostAsJsonAsync("/priorauth", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await response.Content.ReadFromJsonAsync<JsonElement>();
            var requestId = created.GetProperty("id").GetInt32();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var outbox = await db.OutboxMessages
                .Where(m => m.MessageType == nameof(PriorAuthSubmittedMessage))
                .ToListAsync();

            var message = outbox.SingleOrDefault(m =>
                JsonSerializer.Deserialize<PriorAuthSubmittedMessage>(m.Payload)!.PriorAuthRequestId == requestId);

            message.Should().NotBeNull("submitting a request must atomically enqueue its evaluation message");
            message!.ProcessedAt.Should().BeNull("delivery is the dispatcher's job, not the endpoint's");
            message.CorrelationId.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task OutboxDispatcher_DeliversPendingMessageAndMarksProcessed()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var row = new OutboxMessage
            {
                MessageType = nameof(PriorAuthSubmittedMessage),
                Payload = JsonSerializer.Serialize(new PriorAuthSubmittedMessage { PriorAuthRequestId = 999 }),
                CorrelationId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
            db.OutboxMessages.Add(row);
            await db.SaveChangesAsync();

            var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();
            var delivered = await dispatcher.ProcessPendingAsync();

            delivered.Should().BeGreaterThanOrEqualTo(1);

            await db.Entry(row).ReloadAsync();
            row.ProcessedAt.Should().NotBeNull();
            row.AttemptCount.Should().Be(1);
            row.LastError.Should().BeNull();

            var senderMock = Mock.Get(scope.ServiceProvider.GetRequiredService<ServiceBusSender>());
            senderMock.Verify(s => s.SendMessageAsync(
                It.Is<ServiceBusMessage>(m =>
                    m.CorrelationId == row.CorrelationId &&
                    m.MessageId == row.Id.ToString()),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}