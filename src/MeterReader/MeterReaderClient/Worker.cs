using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using MeterReaderWeb.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeterReaderClient {
  public class Worker : BackgroundService {
    private readonly IReadingFactory _readingFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<Worker> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly Lazy<MeterReadingService.MeterReadingServiceClient> _client;

    public Worker(
      IReadingFactory readingFactory, 
      IConfiguration config, 
      ILogger<Worker> logger,
      ILoggerFactory loggerFactory
    ) {
      _readingFactory = readingFactory;
      _config = config;
      _logger = logger;
      _loggerFactory = loggerFactory;
      _client = new Lazy<MeterReadingService.MeterReadingServiceClient>(InitClient);
    }

    private MeterReadingService.MeterReadingServiceClient InitClient() {
      var opts = new GrpcChannelOptions() {
        LoggerFactory = _loggerFactory
      };
      var channel = GrpcChannel.ForAddress(_config["Service:ServerUrl"], opts);
      return new MeterReadingService.MeterReadingServiceClient(channel);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
      var counter = 0;
      var customerId = _config.GetValue<int>("Service:CustomerId");
      var delay = _config.GetValue<int>("Service:DelayInterval");

      while (!stoppingToken.IsCancellationRequested) {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        counter++;

        if (counter % 10 == 0) {
          Console.WriteLine("Sending Diagnostics");
          var stream = _client.Value.SendDiagnostics();
          for (int i = 0; i < 5; i++) {
            var reading = await _readingFactory.GenerateAsync(customerId);
            await stream.RequestStream.WriteAsync(reading);
          }

          await stream.RequestStream.CompleteAsync();
        }

        var pkt = new ReadingPacket() {
          Successful = ReadingStatus.Success,
          Notes = "This is our test"
        };

        for (int i = 0; i < 5; i++) {
          pkt.Readings.Add(await _readingFactory.GenerateAsync(customerId));
        }

        var result = await _client.Value.AddReadingAsync(pkt);

        if (result.Succes == ReadingStatus.Success) {
          _logger.LogInformation("Successfully Sent");
        } else {
          _logger.LogInformation("Failed to Send");
        }

        await Task.Delay(delay, stoppingToken);
      }
    }
  }
}
