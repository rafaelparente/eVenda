using System;

using Microsoft.Extensions.Configuration;

namespace Utils
{
    public class ConfigUtils
    {
        private static IConfiguration Configuration { get; }

        static ConfigUtils()
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");

            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();

            if (isDevelopment)
            {
                builder.AddUserSecrets<ConfigUtils>();
            }

            Configuration = builder.Build();
        }

        public static string GetConnectionString()
        {
            return Configuration.GetConnectionString("ServiceBusConnection");
        }

        public static string GetQueueName()
        {
            return Configuration.GetConnectionString("ServiceBusQueue");
        }
    }
}
