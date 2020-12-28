using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace simple_netcore_processor.Services {
    public interface IStreamProcessor {
         Task process (IConfiguration config);
    }
}