using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

namespace simple_netcore_source.Services
{
    public interface INStreamSource
    {
        Task exec(IConfiguration config, IServiceProvider services);
    }
}