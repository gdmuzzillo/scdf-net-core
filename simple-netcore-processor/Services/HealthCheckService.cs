namespace simple_netcore_processor.Services {
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