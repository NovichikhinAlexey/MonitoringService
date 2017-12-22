namespace Core.Settings
{
    public class SettingsWrapper
    {
        public BaseSettings MonitoringService { get; set; }

        public SlackNotificationSettings SlackNotifications { get; set; }
    }
}
