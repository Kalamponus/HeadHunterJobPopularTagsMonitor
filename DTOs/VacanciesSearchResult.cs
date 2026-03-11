namespace HeadHunterJobPopularTagsMonitor.DTOs
{
    public record VacanciesSearchResult
    {
        public string[] VacanciesIds { get; set; } = [];
        public int AvailablePagesCount { get; set; } = 0;
    }
}
