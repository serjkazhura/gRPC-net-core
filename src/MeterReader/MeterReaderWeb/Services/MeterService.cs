using Grpc.Core;
using MeterReaderWeb.Data;
using MeterReaderWeb.Data.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeterReaderWeb.Services
{
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
                }
                catch (Exception ex) {
                    result.Message = "Excption thrown";
                    _logger.LogError($"Error thrown during AddReading {ex.Message}");
                }
            }

            return result;
        }
    }
}
