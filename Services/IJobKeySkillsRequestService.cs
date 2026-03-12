namespace HeadHunterJobPopularTagsMonitor.Services
{
    public interface IJobKeySkillsRequestService
    {
        Task<Dictionary<string, int>> GetKeySkillsForJobAsync(string jobName, int vacanciesToProcessCount, CancellationToken token = default);
    }
}
