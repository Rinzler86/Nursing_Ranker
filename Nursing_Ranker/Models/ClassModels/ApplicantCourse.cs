using Nursing_Ranker.Models.ClassModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ApplicantCourse
{
    [Key]
    public int ApplicantCourseId { get; set; }

    [ForeignKey("Applicant")]
    public int ApplicantId { get; set; }

    public virtual Applicant Applicant { get; set; }

    [Required]
    public string CourseName { get; set; } = string.Empty;

    public string? Grade { get; set; } // Null means incomplete

    public DateTime? CompletionDate { get; set; }

    public bool Completed { get; set; }

    public bool RequiresRetake { get; set; }

    public int PointsAwarded { get; set; }

    public int CalculatePoints()
    {
        // No grade means no points.
        if (string.IsNullOrEmpty(Grade))
            return 0;

        // Specific logic for Biology 2010 and 2020
        if (CourseName == "BIOL 2010" || CourseName == "BIOL 2020")
        {
            if (Grade == "A")
                return 500;
            if (Grade == "B")
                return 250;
            return 0;
        }

        // General logic for other courses
        if (Applicant != null && Applicant.AllCoursesCompleted && CompletionDate.HasValue && CompletionDate.Value.AddYears(5) >= DateTime.Now)
        {
            return 500;
        }

        return 0;
    }
}
