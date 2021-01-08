namespace simple_netcore_source.Services {
    public class HealthCheckService : IHealthCheckService
     {
        public string check () {
            return "ok";
        }

        public string info () {
            return "ok";
        }

    }
}