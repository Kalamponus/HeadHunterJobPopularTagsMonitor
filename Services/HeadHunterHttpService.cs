using HeadHunterJobPopularTagsMonitor.DTOs;
using System.Text.Json;

namespace HeadHunterJobPopularTagsMonitor.Services
{
	public class HeadHunterHttpService(HttpClient httpClient) : IHeadHunterHttpService
	{
		private const int RequestMaxRetries = 5;
		private const int RetryDelayModifier = 2;
		private const int BaseRetryDelayInMilliseconds = 100;

		private readonly HttpClient _httpClient = httpClient;

		public async Task<VacanciesSearchResult> GetVacanciesIdsAsync(string jobName, int perPage, int currentPage, CancellationToken token = default)
		{
            HttpResponseMessage response = await SendGetRequestWithRetriesAsync($"vacancies?text={Uri.EscapeDataString(jobName)}&per_page={perPage}&page={currentPage}", token);

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

		public async Task<string[]> GetVacancyKeySkillsNamesAsync(string vacancyId, CancellationToken token = default)
		{
			HttpResponseMessage response = await SendGetRequestWithRetriesAsync($"vacancies/{vacancyId}", token);

			using Stream stream = await response.Content.ReadAsStreamAsync(token);
			using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: token);

			string[] keySkillsNames = document.RootElement
				.GetProperty("key_skills")
				.EnumerateArray()
				.Select(skill => skill.GetProperty("name").GetString())
				.Where(name => name != null)
				.Select(name => name!)
				.ToArray();

			return keySkillsNames;
		}

		private async Task<HttpResponseMessage> SendGetRequestWithRetriesAsync(string request, CancellationToken token = default)
		{
			HttpResponseMessage response;
			int retryDelayInMilliseconds = BaseRetryDelayInMilliseconds;
			int requestAttempts = 1;
			
			while (true)
			{
				response = await _httpClient.GetAsync(request, token);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
				{
					if (requestAttempts > RequestMaxRetries)
						break;

					if (response.Headers.RetryAfter?.Delta is TimeSpan retryAfter)
					{
						await Task.Delay(retryAfter, token);
					}
					else
					{
						await Task.Delay(retryDelayInMilliseconds, token);
						retryDelayInMilliseconds *= RetryDelayModifier;
					}

					requestAttempts++;
				}
				else
				{
					break;
				}
			}

			response.EnsureSuccessStatusCode();
			return response;
		}
	}
}
