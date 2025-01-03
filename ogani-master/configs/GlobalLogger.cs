using Microsoft.Extensions.Logging;

namespace ogani_master.configs
{
    public static class GlobalLogger
    {
        public static ILoggerFactory LoggerFactory { get; set; } = null!;
        public static ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }

}
