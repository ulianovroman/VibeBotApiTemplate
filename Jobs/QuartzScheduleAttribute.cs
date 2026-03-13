namespace BotApiTemplate.Jobs
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class QuartzScheduleAttribute : Attribute
    {
        public QuartzScheduleAttribute(string cronExpression)
        {
            CronExpression = cronExpression;
        }

        public string CronExpression { get; }
        public string TimeZoneId { get; init; } = "UTC";
    }
}
