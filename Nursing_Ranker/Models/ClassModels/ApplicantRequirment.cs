using Nursing_Ranker.Models.ClassModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ApplicantRequirement
{
    [Key]
    public int RequirementId { get; set; }

    [ForeignKey("Applicant")]
    public int ApplicantId { get; set; }
    public virtual Applicant Applicant { get; set; }

    [Required]
    public string TestName { get; set; } = string.Empty; // ACT or HESI

    // Make Score nullable but default to 0.
    public decimal? Score { get; set; }

    // Date when the test was taken.
    public DateTime? TestDate { get; set; }

    public int PointsAwarded => CalculatePoints();

    private int CalculatePoints()
    {
        // If test date is older than 5 years, return 0.
        if (TestDate.HasValue && TestDate.Value.AddYears(5) < DateTime.Now)
        {
            return 0;
        }

        // Use 0 if Score is null.
        decimal score = Score ?? 0;

        if (TestName == "ACT")
        {
            return score >= 26 ? 500 : score >= 19 ? 250 : 0;
        }
        else if (TestName == "HESI")
        {
            return score >= 80 ? 500 : 0;
        }
        return 0;
    }
}


