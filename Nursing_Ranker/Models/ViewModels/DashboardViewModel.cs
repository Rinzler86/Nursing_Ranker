using Nursing_Ranker.Models.ClassModels;

namespace Nursing_Ranker.Models.ViewModels
{
    public class DashboardViewModel
    {
        public IEnumerable<Applicant> Applicants { get; set; } = new List<Applicant>();

        // Updated: Include applicant courses instead of global courses
        public IEnumerable<ApplicantCourse> ApplicantCourses { get; set; } = new List<ApplicantCourse>();

        // Updated: Include applicant test scores instead of global requirements
        public IEnumerable<ApplicantRequirement> ApplicantRequirements { get; set; } = new List<ApplicantRequirement>();
    }
}


