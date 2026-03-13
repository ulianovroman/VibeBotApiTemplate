using System.Reflection;
using Quartz;

namespace BotApiTemplate.Jobs
{
    public static class QuartzRegistrationExtensions
    {
        public static IServiceCollection AddAttributedQuartzJobs(
            this IServiceCollection services,
            Assembly assembly)
        {
            services.AddQuartz(options =>
            {
                foreach (var (jobType, schedule) in FindAttributedJobs(assembly))
                {
                    var jobKey = new JobKey(jobType.FullName ?? jobType.Name);
                    var triggerKey = new TriggerKey($"{jobKey.Name}-trigger");

                    options.AddJob(jobType, configure =>
                        configure.WithIdentity(jobKey));

                    options.AddTrigger(configure => configure
                        .ForJob(jobKey)
                        .WithIdentity(triggerKey)
                        .WithCronSchedule(
                            schedule.CronExpression,
                            cron => cron.InTimeZone(ResolveTimeZone(schedule.TimeZoneId))));
                }
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            return services;
        }

        private static IEnumerable<(Type JobType, QuartzScheduleAttribute Schedule)> FindAttributedJobs(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && typeof(IJob).IsAssignableFrom(type))
                .Select(type => new
                {
                    Type = type,
                    Schedule = type.GetCustomAttribute<QuartzScheduleAttribute>()
                })
                .Where(item => item.Schedule is not null)
                .Select(item => (item.Type, item.Schedule!));
        }

        private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
    }
}
