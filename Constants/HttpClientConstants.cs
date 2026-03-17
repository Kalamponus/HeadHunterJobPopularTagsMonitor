using System.Reflection;

namespace HeadHunterJobPopularTagsMonitor.Constants
{
    public static class HttpClientConstants
    {
        public static readonly string UserAgent = $"HeadhunterKeySkillsMonitor/{Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0"} (.NET; {Environment.OSVersion.Platform})";
    }
}
