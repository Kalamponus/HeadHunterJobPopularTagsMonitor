using System;
using System.Reflection;

namespace HeadHunterJobPopularTagsMonitor.Constants
{
    public static class HttpClientConstants
    {
        public static readonly string UserAgent = $"MyApp/{Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0"} (m.maiorov@inbox.ru) (.NET; {Environment.OSVersion.Platform})";
    }
}
