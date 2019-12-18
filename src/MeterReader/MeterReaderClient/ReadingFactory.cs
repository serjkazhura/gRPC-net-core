using Google.Protobuf.WellKnownTypes;
using MeterReaderWeb.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MeterReaderClient {
  internal sealed class ReadingFactory : IReadingFactory {
    private readonly ILogger<ReadingFactory> _logger;

    public ReadingFactory(ILogger<ReadingFactory> logger) {
      _logger = logger;
    }

    public Task<ReadingMessage> GenerateAsync(int customerId) {
      var reading = new ReadingMessage() {
        CustomerId = customerId,
        ReadingValue = new Random().Next(10000),
        ReadingTime = Timestamp.FromDateTime(DateTime.UtcNow)
      };
      return Task.FromResult(reading);
    }
  }
}
