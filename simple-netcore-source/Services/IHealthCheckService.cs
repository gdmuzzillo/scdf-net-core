namespace simple_netcore_processor.Services
{
    public interface IHealthCheckService
    {
        string check();
        string info();
    }
}