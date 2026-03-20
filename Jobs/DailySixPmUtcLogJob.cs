using Quartz;

namespace VibeBotApi.Jobs
{
    [QuartzSchedule("0 0 18 * * ?", TimeZoneId = "UTC")]
    public sealed class DailySixPmUtcLogJob : IJob
    {
        private readonly ILogger<DailySixPmUtcLogJob> _logger;

        public DailySixPmUtcLogJob(ILogger<DailySixPmUtcLogJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Quartz job DailySixPmUtcLogJob triggered at {TriggeredAtUtc}", DateTime.UtcNow);
            return Task.CompletedTask;
        }
    }
}
