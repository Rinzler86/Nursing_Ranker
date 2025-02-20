using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nursing_Ranker.Models.ClassModels
{
    public class Applicant
    {
        [Key]
        public int ApplicantId { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? WNumber { get; set; }

        [Range(0.00, 4.00)]
        public decimal WSCCGPA { get; set; }

        [Range(0.00, 4.00)]
        public decimal? NursingGPA { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pre-Applicant";

        public string MissingRequirements { get; set; } = string.Empty;

        // New property: extra credits beyond required courses.
        public int ExtraCredits { get; set; } = 0;

        // Computed property for extra credits points based on your rules:
        // 0 - 11 credits => 0 pts; 12 - 26 => 500 pts; 27+ => 1000 pts.
        [NotMapped]
        public int ExtraCreditsPoints => (ExtraCredits >= 27) ? 1000 : (ExtraCredits >= 12 ? 500 : 0);

        // TotalPoints now includes extra credits points.
        public int TotalPoints => TotalCoursePoints + TotalTestPoints + (AllCoursesCompleted ? 500 : 0) + ExtraCreditsPoints;

        // Adjust the divisor if needed based on the maximum possible score.
        [Range(0.00, 100.00)]
        public decimal NormalizedScore => Math.Round((TotalPoints / 3500m) * 100, 2);

        // Navigation properties
        public virtual ICollection<ApplicantCourse> ApplicantCourses { get; set; } = new List<ApplicantCourse>();
        public virtual ICollection<ApplicantRequirement> ApplicantRequirements { get; set; } = new List<ApplicantRequirement>();

        // Derived property: A course is considered complete if:
        // - A grade is entered,
        // - A completion date exists and is within the past 5 years,
        // - And the grade is A, B, or C.
        [NotMapped]
        public bool AllCoursesCompleted => ApplicantCourses.All(c =>
            !string.IsNullOrEmpty(c.Grade) &&
            c.CompletionDate.HasValue &&
            c.CompletionDate.Value.AddYears(5) >= DateTime.Now &&
            (c.Grade == "A" || c.Grade == "B" || c.Grade == "C")
        );

        public int TotalCoursePoints => ApplicantCourses.Sum(c => c.PointsAwarded);
        public int TotalTestPoints => ApplicantRequirements.Sum(r => r.PointsAwarded);

        [NotMapped]
        public bool MeetsMinimumRequirements
        {
            get
            {
                // Define a simple grade mapping for GPA calculation.
                var gradeMapping = new Dictionary<string, decimal>
                {
                    { "A", 4.0m },
                    { "B", 3.0m },
                    { "C", 2.0m },
                    { "D", 1.0m },
                    { "F", 0.0m }
                };

                // Check required courses with a grade of C or better.
                bool biol2010Ok = ApplicantCourses.Any(c => c.CourseName == "BIOL 2010" &&
                    (c.Grade == "A" || c.Grade == "B" || c.Grade == "C") &&
                    c.CompletionDate.HasValue && c.CompletionDate.Value.AddYears(5) >= DateTime.Now);
                bool biol2011Ok = ApplicantCourses.Any(c => c.CourseName == "BIOL 2011" &&
                    (c.Grade == "A" || c.Grade == "B" || c.Grade == "C") &&
                    c.CompletionDate.HasValue && c.CompletionDate.Value.AddYears(5) >= DateTime.Now);
                bool engl1010Ok = ApplicantCourses.Any(c => c.CourseName == "ENGL 1010" &&
                    (c.Grade == "A" || c.Grade == "B" || c.Grade == "C") &&
                    c.CompletionDate.HasValue && c.CompletionDate.Value.AddYears(5) >= DateTime.Now);
                bool math1530Ok = ApplicantCourses.Any(c => c.CourseName == "MATH 1530" &&
                    (c.Grade == "A" || c.Grade == "B" || c.Grade == "C") &&
                    c.CompletionDate.HasValue && c.CompletionDate.Value.AddYears(5) >= DateTime.Now);
                bool psyc1030Ok = ApplicantCourses.Any(c => c.CourseName == "PSYC 1030" &&
                    (c.Grade == "A" || c.Grade == "B" || c.Grade == "C") &&
                    c.CompletionDate.HasValue && c.CompletionDate.Value.AddYears(5) >= DateTime.Now);

                // For required general education GPA, assume these courses are required:
                var requiredGenEd = new[] { "BIOL 2010", "BIOL 2011", "ENGL 1010", "MATH 1530", "PSYC 1030" };
                var genEdCourses = ApplicantCourses.Where(c => requiredGenEd.Contains(c.CourseName) &&
                    !string.IsNullOrEmpty(c.Grade));
                decimal avgGenEdGPA = genEdCourses.Any() ? genEdCourses.Average(c => gradeMapping.ContainsKey(c.Grade) ? gradeMapping[c.Grade] : 0.0m) : 0.0m;
                bool meetsGenEdGPA = avgGenEdGPA >= 2.5m;

                // Overall Walters State GPA requirement.
                bool overallGPAOk = WSCCGPA >= 2.0m;

                // For learning support courses and applications, either add properties or assume true for now.
                bool appliedToWSCC = true;
                bool appliedToNursing = true;
                bool completedLearningSupport = true;

                return appliedToWSCC
                       && appliedToNursing
                       && completedLearningSupport
                       && biol2010Ok && biol2011Ok && engl1010Ok && math1530Ok && psyc1030Ok
                       && meetsGenEdGPA
                       && overallGPAOk;
            }
        }

        [NotMapped]
        public string IneligibilityReasons
        {
            get
            {
                var reasons = new List<string>();

                // Check WSCC GPA requirement (minimum 2.0)
                if (WSCCGPA < 2.0m)
                    reasons.Add("WSCC GPA is below 2.0");

                // Define required courses and their minimum passing grade (C or better)
                var requiredCourses = new[] { "BIOL 2010", "BIOL 2011", "ENGL 1010", "MATH 1530", "PSYC 1030" };
                var gradeMapping = new Dictionary<string, decimal>
                {
                    { "A", 4.0m },
                    { "B", 3.0m },
                    { "C", 2.0m },
                    { "D", 1.0m },
                    { "F", 0.0m }
                };

                // Check each required course:
                foreach (var req in requiredCourses)
                {
                    var course = ApplicantCourses.FirstOrDefault(c => c.CourseName == req);
                    if (course == null ||
                        string.IsNullOrEmpty(course.Grade) ||
                        !course.CompletionDate.HasValue ||
                        course.CompletionDate.Value.AddYears(5) < DateTime.Now ||
                        (gradeMapping.ContainsKey(course.Grade) && gradeMapping[course.Grade] < 2.0m))
                    {
                        reasons.Add($"{req} is incomplete or below a C");
                    }
                }

                // Check if required general education GPA is met
                var genEdCourses = ApplicantCourses.Where(c => requiredCourses.Contains(c.CourseName) &&
                                            !string.IsNullOrEmpty(c.Grade) &&
                                            c.CompletionDate.HasValue &&
                                            c.CompletionDate.Value.AddYears(5) >= DateTime.Now);
                if (genEdCourses.Any())
                {
                    var avgGPA = genEdCourses.Average(c => gradeMapping.ContainsKey(c.Grade) ? gradeMapping[c.Grade] : 0m);
                    if (avgGPA < 2.5m)
                        reasons.Add("Average GPA for required courses is below 2.5");
                }
                else
                {
                    reasons.Add("No valid general education courses found");
                }

                return reasons.Any() ? string.Join("; ", reasons) : "All minimum requirements met";
            }
        }
    }
}


