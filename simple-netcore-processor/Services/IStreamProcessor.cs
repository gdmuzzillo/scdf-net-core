using Microsoft.Extensions.Configuration;
namespace simple_netcore_processor.Services {
    public interface IStreamProcessor {
         void process (IConfiguration config);
    }
}