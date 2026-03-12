using HeadHunterJobPopularTagsMonitor.DTOs;

namespace HeadHunterJobPopularTagsMonitor.Services
{
    public interface IHeadHunterHttpService
    {
        Task<VacanciesSearchResult> GetVacanciesIdsAsync(string jobName, int per_page, int currentPage, CancellationToken token = default);
        Task<string[]> GetVacancyKeySkillsNamesAsync(string vacancyId, CancellationToken token = default);
    }
}
