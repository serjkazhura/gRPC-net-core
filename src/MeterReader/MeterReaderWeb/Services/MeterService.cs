using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MeterReaderWeb.Data;
using MeterReaderWeb.Data.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MeterReaderWeb.Services {
  public class MeterService : MeterReadingService.MeterReadingServiceBase
    {
        private readonly IReadingRepository _readingRepository;
        private readonly ILogger<MeterService> _logger;

        public MeterService(
            IReadingRepository readingRepository,
            ILogger<MeterService> logger
        ) {
            _readingRepository = readingRepository;
            _logger = logger;
        }

    public override async Task<Empty> SendDiagnostics(
      IAsyncStreamReader<ReadingMessage> requestStream, 
      ServerCallContext context
    ) {

      var task = Task.Run(async () => {
        await foreach (var reading in requestStream.ReadAllAsync()) {
          _logger.LogInformation($"Received Reading: {reading}");
        }
      });

      await task;

      return new Empty();
    }

    public override async Task<StatusMessage> AddReading(
      ReadingPacket request,
      ServerCallContext context
    ) {
      var result = new StatusMessage() {
        Succes = ReadingStatus.Failure
      };

      if (request.Successful == ReadingStatus.Success) {
        try {
          foreach (var reading in request.Readings) {
            var r = new MeterReading() {
              Value = reading.ReadingValue,
              ReadingDate = reading.ReadingTime.ToDateTime(),
              CustomerId = reading.CustomerId
            };
            _readingRepository.AddEntity(r);
          }
          if (await _readingRepository.SaveAllAsync()) {
            result.Succes = ReadingStatus.Success;
          }
        } catch (Exception ex) {
          result.Message = "Excption thrown";
          _logger.LogError($"Error thrown during AddReading {ex.Message}");
        }
      }

      return result;
    }
  }
}
