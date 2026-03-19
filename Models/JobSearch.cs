using HeadHunterJobPopularTagsMonitor.Constants;
using System.ComponentModel.DataAnnotations;

namespace HeadHunterJobPopularTagsMonitor.Models
{
    public class JobSearch
    {
        public const int JobNameMaxLength = 200;

        [Required(ErrorMessage = "Job name field should not be empty")]
        [StringLength(JobNameMaxLength, ErrorMessage = "Job name max length is exceeded")]
        public string JobName { get; set; } = "Программист C#";

        [Required(ErrorMessage = "Vacancies count field should not be empty")]
        [Range(1, HeadHunterApiConstants.MaxVacanciesInRequest, ErrorMessage = "Vacancies count should be greater than 1 and within max range")]
        public int VacanciesCount { get; set; } = 100;
    }
}
