using Microsoft.Extensions.Configuration;

namespace vendtechext.Helper
{
    public static class DomainEnvironment
    {
        private static readonly string _databaseName;
        private static readonly IConfiguration _configuration;

        static DomainEnvironment()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            _configuration = config;
            _databaseName = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public static bool IsProduction => _databaseName.Contains("VENDTECHEXT_MAIN", StringComparison.OrdinalIgnoreCase);

        public static bool IsSandbox => _databaseName.Contains("VENDTECHEXT_DEV", StringComparison.OrdinalIgnoreCase);

        public static string GetEnvironment()
        {
            if (IsProduction) return "Production";
            if (IsSandbox) return "Sandbox";
            return "Unknown";
        }
        public static string DashboardUrl => _configuration["Client:BaseUrl"];
        public static string APIUrl => IsProduction ? _configuration["Client:Production"] : _configuration["Client:Sandbox"];
        public static IConfiguration Configuration => _configuration;
    }

}
