using HeadHunterJobPopularTagsMonitor.DTOs;
using System.Text.Json;

namespace HeadHunterJobPopularTagsMonitor.Services
{
	public class HeadHunterHttpService(HttpClient httpClient)
	{
		private readonly HttpClient _httpClient = httpClient;

		public async Task<VacanciesSearchResult> GetVacanciesIdsAsync(string jobName, int per_page, int currentPage, CancellationToken token)
		{
			HttpResponseMessage response = await _httpClient.GetAsync($"vacancies?text={jobName}&per_page={per_page}&page={currentPage}", token);

			response.EnsureSuccessStatusCode();

			VacanciesSearchResult searchResult = new();

            using Stream stream = await response.Content.ReadAsStreamAsync(token);
            using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: token);

            searchResult.VacanciesIds = document.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Select(vacancy => vacancy.GetProperty("id").GetString())
                .Where(id => id != null)
                .Select(id => id!)
                .ToArray();

            searchResult.AvailablePagesCount = document.RootElement
                .GetProperty("pages")
                .GetInt32();

            return searchResult;
		}

		public async Task<string[]> GetVacancyKeySkillsNamesAsync(string vacancyId, CancellationToken token)
		{
			HttpResponseMessage response = await _httpClient.GetAsync($"vacancies/{vacancyId}", token);

			response.EnsureSuccessStatusCode();

			string[] keySkillsNames = [];

            using Stream stream = await response.Content.ReadAsStreamAsync(token);
            using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: token);

            keySkillsNames = document.RootElement
                .GetProperty("key_skills")
                .EnumerateArray()
                .Select(skill => skill.GetProperty("name").GetString())
                .Where(name => name != null)
                .Select(name => name!)
                .ToArray();

            return keySkillsNames;
		}
	}
}
