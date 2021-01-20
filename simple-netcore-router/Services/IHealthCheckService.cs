namespace simple_netcore_router.Services
{
    public interface IHealthCheckService
    {
        string check();
        string info();
    }
}