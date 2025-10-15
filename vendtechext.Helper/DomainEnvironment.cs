using Microsoft.Extensions.Configuration;

namespace vendtechext.SDK
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

        public static bool IsExtProduction => _databaseName.Contains("VENDTECHEXT_MAIN", StringComparison.OrdinalIgnoreCase);

        public static bool IsExtSandbox => _databaseName.Contains("VENDTECHEXT_DEV", StringComparison.OrdinalIgnoreCase);

        public static string GetEnvironment()
        {
            if (IsExtProduction) return "Production";
            if (IsExtSandbox) return "Sandbox";
            return "Unknown";
        }
        public static string DashboardUrl => _configuration["Client:BaseUrl"];
        public static string APIUrl => IsExtProduction ? _configuration["Client:Production"] : _configuration["Client:Sandbox"];
        public static IConfiguration Configuration => _configuration;
        public static string DefaultGateway => _configuration["Client:DefaultGateway"];
        public static string KwikTalkUrl => _configuration["KwikTalk:url"];
    }

}
