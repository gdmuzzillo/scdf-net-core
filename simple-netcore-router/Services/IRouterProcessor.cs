using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace simple_netcore_router.Services {
    public interface IRouterProcessor {
         Task process (IConfiguration config);
    }
}