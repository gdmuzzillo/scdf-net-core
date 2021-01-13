using System;
using com.avro.bean;
namespace simple_netcore_source.Services
{
    public interface IDataService
    {
        Order[] readData();
    }
}