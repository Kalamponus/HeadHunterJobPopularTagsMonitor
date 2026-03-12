using HeadHunterJobPopularTagsMonitor.DTOs;

namespace HeadHunterJobPopularTagsMonitor.Services
{
	public class JobKeySkillsRequestService(HeadHunterHttpService headHunterHttpService) : IJobKeySkillsRequestService, IDisposable
    {
		// Right now HH cant return more than 100 vacancies per page and is limited by 2000 vacancies total
		private const int VacanciesPerPage = 100;
		private const int MaxParallelRequests = 10;

		private readonly HeadHunterHttpService _headHunterHttpService = headHunterHttpService;

		private readonly SemaphoreSlim _semaphore = new(MaxParallelRequests);

		/// <summary>
		/// Gets popular key skills from vacancies that were found by job name.
		/// </summary>
		/// <param name="jobName">A job name to search vacancies for.</param>
		/// <param name="vacanciesToProcessCount">Amount of vacancies to take key skills from.
		/// If it is greater than the actual result (and max vacancies given by API response).</param>
		/// <returns>Found key skills from vacancies, sorted by their popularity and how many times they were encountered. There can be less vacancies to take key skills from in case of 'vacanciesToProcessCount'
		/// parameter being greater than the actual result.</returns>
		public async Task<Dictionary<string, int>> GetKeySkillsForJobAsync(string jobName, int vacanciesToProcessCount, CancellationToken token = default)
		{
			if (string.IsNullOrWhiteSpace(jobName))
				return [];

			if (vacanciesToProcessCount <= 0)
				return [];

			VacanciesSearchResult searchResult;

			searchResult = await _headHunterHttpService.GetVacanciesIdsAsync(jobName, VacanciesPerPage, 0, token);

			if (searchResult.AvailablePagesCount == 0)
				return [];

			int actualRemainingPagesCount = vacanciesToProcessCount > VacanciesPerPage
				? Math.Min(searchResult.AvailablePagesCount,
				vacanciesToProcessCount / VacanciesPerPage + (vacanciesToProcessCount % VacanciesPerPage > 0 ? 1 : 0)) - 1
				: 0;

			int remainingVacanciesToProcessCount = vacanciesToProcessCount;

			List<string> vacanciesIds = new(GetVacanciesIdsFromPage(searchResult.VacanciesIds, ref remainingVacanciesToProcessCount));

			if (actualRemainingPagesCount > 0 && remainingVacanciesToProcessCount > 0)
			{
				IEnumerable<Task<VacanciesSearchResult>> pageTasks = Enumerable.Range(1, actualRemainingPagesCount)
					.Select(async pageNumber =>
					{
						await _semaphore.WaitAsync(token);

						try
						{
							return await _headHunterHttpService.GetVacanciesIdsAsync(jobName, VacanciesPerPage, pageNumber, token);
						}
						catch (HttpRequestException ex)
						{
							return new VacanciesSearchResult();
                        }
						finally
						{
							_semaphore.Release();
						}
					});

				VacanciesSearchResult[] pageResults = await Task.WhenAll(pageTasks);

				foreach (VacanciesSearchResult result in pageResults)
				{
					if (result.VacanciesIds.Length > 0)
						vacanciesIds.AddRange(GetVacanciesIdsFromPage(result.VacanciesIds, ref remainingVacanciesToProcessCount));
				}
			}

			IEnumerable<Task<string[]>> skillTasks = vacanciesIds.Select(async vacancyId =>
			{
				await _semaphore.WaitAsync(token);

				try
				{
					return await _headHunterHttpService.GetVacancyKeySkillsNamesAsync(vacancyId, token);
				}
                catch (HttpRequestException ex)
                {
					return [];
                }
                finally
				{
					_semaphore.Release();
				}
			});

			// An array of collections of skills that are mentioned inside each vacancy
			string[][] keySkillsInVacancies = await Task.WhenAll(skillTasks);

			Dictionary<string, int> keySkills = [];

			foreach (string[] skillsInVacancy in keySkillsInVacancies)
			{
				foreach (string skill in skillsInVacancy)
				{
					if (!keySkills.TryAdd(skill, 1))
						keySkills[skill]++;
				}
			}

			return keySkills.OrderByDescending(skill => skill.Value).ToDictionary();
		}

        private string[] GetVacanciesIdsFromPage(string[] vacanciesOnPageIds, ref int remainingVacanciesToProcessCount)
		{
			string[] vacanciesToStore = remainingVacanciesToProcessCount > vacanciesOnPageIds.Length
							? vacanciesOnPageIds
							: vacanciesOnPageIds[0..remainingVacanciesToProcessCount];

			remainingVacanciesToProcessCount -= vacanciesToStore.Length;

			return vacanciesToStore;
		}

		public void Dispose()
		{
			_semaphore.Dispose();
		}
	}
}
