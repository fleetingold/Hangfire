using Hangfire;

namespace ConsoleSample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(@"server=192.168.1.148;database=OMSServerTimedTask;uid=sa;pwd=ABC!@#abc123");

            using var server = new BackgroundJobServer();
            Console.ReadLine();
        }
    }
}