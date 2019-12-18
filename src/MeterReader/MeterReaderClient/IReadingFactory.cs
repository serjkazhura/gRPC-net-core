using MeterReaderWeb.Services;
using System.Threading.Tasks;

namespace MeterReaderClient {
  public interface IReadingFactory {
    Task<ReadingMessage> GenerateAsync(int customerId);
  }
}