using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.SqlServer;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetCoreSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Information))
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<HostOptions>(option =>
                    {
                        option.ShutdownTimeout = TimeSpan.FromSeconds(60);
                    });

                    services.TryAddSingleton<SqlServerStorageOptions>(new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.FromTicks(1),
                        UseRecommendedIsolationLevel = true,
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(1)
                    });

                    services.TryAddSingleton<IBackgroundJobFactory>(x => new CustomBackgroundJobFactory(
                        new BackgroundJobFactory(x.GetRequiredService<IJobFilterProvider>())));

                    services.TryAddSingleton<IBackgroundJobPerformer>(x => new CustomBackgroundJobPerformer(
                        new BackgroundJobPerformer(
                            x.GetRequiredService<IJobFilterProvider>(),
                            x.GetRequiredService<JobActivator>(),
                            TaskScheduler.Default)));

                    services.TryAddSingleton<IBackgroundJobStateChanger>(x => new CustomBackgroundJobStateChanger(
                            new BackgroundJobStateChanger(x.GetRequiredService<IJobFilterProvider>())));

                    services.AddHangfire((provider, configuration) => configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseSqlServerStorage(@"server=192.168.1.148;database=OMSServerTimedTask;uid=sa;pwd=ABC!@#abc123",
                        provider.GetRequiredService<SqlServerStorageOptions>()));

                    services.AddHostedService<RecurringJobsService>();
                    services.AddHangfireServer(options =>
                    {
                        options.StopTimeout = TimeSpan.FromSeconds(15);
                        options.ShutdownTimeout = TimeSpan.FromSeconds(30);
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}
