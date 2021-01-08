namespace simple_netcore_source.Services
{
    public interface IHealthCheckService
    {
        string check();
        string info();
    }
}