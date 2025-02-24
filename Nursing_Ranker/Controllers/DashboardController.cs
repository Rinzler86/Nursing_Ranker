using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Nursing_Ranker.Data;
using Nursing_Ranker.Models.ClassModels;
using Nursing_Ranker.Models.Requests;
using Nursing_Ranker.Models.ViewModels;

namespace Nursing_Ranker.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var user = _context.Users.Find(userId.Value);
                ViewBag.User = user;
            }
            base.OnActionExecuting(context);
        }

        // Redirect Index to Dashboard so that a model is always provided.
        public IActionResult Index()
        {
            _logger.LogInformation("Index action called.");
            return RedirectToAction("Dashboard");
        }

        // Updated Dashboard action to fetch correct data
        public IActionResult Dashboard()
        {
            _logger.LogInformation("Dashboard action called.");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                _logger.LogInformation("User ID found in session: {UserId}", userId.Value);
                var user = _context.Users.Find(userId.Value);
                if (user != null)
                {
                    _logger.LogInformation("User found: {UserEmail}", user.Email);
                    ViewBag.User = user;
                }
                else
                {
                    _logger.LogWarning("User not found in database for ID: {UserId}", userId.Value);
                }
            }
            else
            {
                _logger.LogWarning("User ID not found in session. Redirecting to login.");
                return RedirectToAction("Login", "User");
            }

            // Load Applicants with related Courses & Requirements
            var model = new DashboardViewModel
            {
                Applicants = _context.Applicants
                    .Include(a => a.ApplicantCourses)
                    .Include(a => a.ApplicantRequirements)
                    .ToList(),

                ApplicantCourses = _context.ApplicantCourses.ToList(),
                ApplicantRequirements = _context.ApplicantRequirements.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddApplicant([FromBody] Applicant applicant)
        {
            _logger.LogInformation("AddApplicant action called.");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model state is valid. Adding applicant: {@Applicant}", applicant);

                // Add default courses (all courses start as incomplete)
                var requiredCourses = CourseScoring.CourseRules.Keys;
                foreach (var courseName in requiredCourses)
                {
                    applicant.ApplicantCourses.Add(new ApplicantCourse
                    {
                        CourseName = courseName,
                        Grade = null, // Default to incomplete
                        CompletionDate = null
                    });
                }

                // Add the applicant to the context and save to generate an ID.
                _context.Applicants.Add(applicant);
                _context.SaveChanges();

                // Now that the applicant has been saved (and ApplicantId is set),
                // add default test rows for ACT and HESI.
                _context.ApplicantRequirements.Add(new ApplicantRequirement
                {
                    ApplicantId = applicant.ApplicantId,
                    TestName = "ACT",
                    Score = 0,
                    TestDate = null
                });
                _context.ApplicantRequirements.Add(new ApplicantRequirement
                {
                    ApplicantId = applicant.ApplicantId,
                    TestName = "HESI",
                    Score = 0,
                    TestDate = null
                });
                _context.SaveChanges();

                _logger.LogInformation("Applicant added successfully with default test rows.");
                return Json(new { success = true });
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage);
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", string.Join(", ", errors));
                return Json(new { success = false, message = "Failed to add applicant. Please check your input." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditApplicant([FromBody] Applicant applicant)
        {
            if (ModelState.IsValid)
            {
                var existingApplicant = _context.Applicants
                    .Include(a => a.ApplicantCourses)
                    .FirstOrDefault(a => a.ApplicantId == applicant.ApplicantId);

                if (existingApplicant != null)
                {
                    existingApplicant.FirstName = applicant.FirstName;
                    existingApplicant.MiddleName = applicant.MiddleName;
                    existingApplicant.LastName = applicant.LastName;
                    existingApplicant.WNumber = applicant.WNumber;
                    existingApplicant.WSCCGPA = applicant.WSCCGPA;
                    // Do not update NursingGPA so it remains unchanged.
                    existingApplicant.Status = applicant.Status;

                    _context.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Applicant not found." });
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Failed to update applicant: " + string.Join("; ", errors) });
            }
        }


        [HttpPost]
        public IActionResult DeleteApplicant(int id)
        {
            var applicant = _context.Applicants
                              .Include(a => a.ApplicantCourses)
                              .Include(a => a.ApplicantRequirements)
                              .FirstOrDefault(a => a.ApplicantId == id);
            if (applicant == null)
            {
                return Json(new { success = false, message = "Applicant not found." });
            }

            // Optionally, remove related entities if cascade delete isn't configured.
            _context.ApplicantCourses.RemoveRange(applicant.ApplicantCourses);
            _context.ApplicantRequirements.RemoveRange(applicant.ApplicantRequirements);
            _context.Applicants.Remove(applicant);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetApplicant(int id)
        {
            _logger.LogInformation("GetApplicant action called with ID: {ApplicantId}", id);
            var applicant = _context.Applicants
                .Include(a => a.ApplicantCourses)
                .Include(a => a.ApplicantRequirements)
                .FirstOrDefault(a => a.ApplicantId == id);
            if (applicant == null)
            {
                _logger.LogWarning("Applicant not found for ID: {ApplicantId}", id);
                return NotFound();
            }
            _logger.LogInformation("Applicant found: {@Applicant}", applicant);
            return Json(new
            {
                applicantId = applicant.ApplicantId,
                firstName = applicant.FirstName,
                middleName = applicant.MiddleName,
                lastName = applicant.LastName,
                wNumber = applicant.WNumber,
                wsccGpa = applicant.WSCCGPA,
                nursingGpa = applicant.NursingGPA,
                status = applicant.Status,
                totalPoints = applicant.TotalPoints,
                normalizedScore = applicant.NormalizedScore,
                missingRequirements = string.IsNullOrEmpty(applicant.MissingRequirements) ? "None" : applicant.MissingRequirements,
                extraCredits = applicant.ExtraCredits,
                extraCreditsPoints = applicant.ExtraCreditsPoints,
                courses = applicant.ApplicantCourses.Select(c => new
                {
                    c.CourseName,
                    c.Grade,
                    CompletionDate = c.CompletionDate?.ToString("yyyy-MM-dd"),
                    c.PointsAwarded,
                    c.Completed
                }),
                tests = applicant.ApplicantRequirements.Select(t => new
                {
                    t.TestName,
                    t.Score,
                    t.PointsAwarded
                })
            });
        }


        [HttpGet]
        public IActionResult GetCourses()
        {
            var courses = _context.ApplicantCourses.Select(c => new
            {
                courseId = c.ApplicantCourseId,
                courseName = c.CourseName
            }).ToList();

            return Json(courses);
        }

        [HttpGet]
        public IActionResult GetApplicantCourses(int id)
        {
            var courses = _context.ApplicantCourses
                .Where(ac => ac.ApplicantId == id)
                .Select(ac => new
                {
                    courseId = ac.ApplicantCourseId,
                    courseName = ac.CourseName,
                    grade = ac.Grade,
                    completionDate = ac.CompletionDate
                })
                .ToList();

            return Json(courses);
        }

        [HttpPost]
        public IActionResult UpdateApplicantCourse([FromBody] ApplicantCourse model)
        {
            var existingCourse = _context.ApplicantCourses
                .FirstOrDefault(ac => ac.ApplicantId == model.ApplicantId && ac.ApplicantCourseId == model.ApplicantCourseId);

            if (existingCourse != null)
            {
                existingCourse.Grade = model.Grade;
                existingCourse.CompletionDate = model.CompletionDate;
            }
            else
            {
                _context.ApplicantCourses.Add(new ApplicantCourse
                {
                    ApplicantId = model.ApplicantId,
                    ApplicantCourseId = model.ApplicantCourseId,
                    Grade = model.Grade,
                    CompletionDate = model.CompletionDate
                });
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateApplicantCourses()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
                _logger.LogInformation("Request body: {RequestBody}", body);
            }

            List<ApplicantCourse> courses;
            try
            {
                courses = JsonConvert.DeserializeObject<List<ApplicantCourse>>(body);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize request body.");
                return BadRequest("Invalid request body format.");
            }

            _logger.LogInformation("UpdateApplicantCourses action called.");

            if (courses == null || !courses.Any())
            {
                _logger.LogWarning("Courses parameter is null or empty.");
                return BadRequest("Courses cannot be null or empty");
            }

            foreach (var model in courses)
            {
                if (model == null)
                {
                    _logger.LogWarning("Null course model encountered.");
                    continue;
                }

                _logger.LogInformation("Processing course: {@Course}", model);

                var existingCourse = _context.ApplicantCourses
                    .FirstOrDefault(ac => ac.ApplicantId == model.ApplicantId && ac.ApplicantCourseId == model.ApplicantCourseId);

                if (existingCourse != null)
                {
                    _logger.LogInformation("Existing course found. Updating course: {@ExistingCourse}", existingCourse);
                    existingCourse.Grade = model.Grade;
                    existingCourse.CompletionDate = model.CompletionDate;
                    existingCourse.PointsAwarded = existingCourse.CalculatePoints();
                }
                else
                {
                    _logger.LogInformation("No existing course found. Adding new course: {@NewCourse}", model);
                    var newCourse = new ApplicantCourse
                    {
                        ApplicantId = model.ApplicantId,
                        ApplicantCourseId = model.ApplicantCourseId,
                        Grade = model.Grade,
                        CompletionDate = model.CompletionDate,
                        PointsAwarded = model.CalculatePoints()
                    };
                    _context.ApplicantCourses.Add(newCourse);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Courses updated successfully.");
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update courses in the database.");
                return StatusCode(500, "Internal server error while updating courses.");
            }
        }



        [HttpGet]
        public IActionResult GetApplicantRequirements(int id)
        {
            var tests = _context.ApplicantRequirements
                .Where(r => r.ApplicantId == id)
                .Select(r => new
                {
                    requirementId = r.RequirementId,
                    testName = r.TestName,
                    score = r.Score,
                    testDate = r.TestDate.HasValue ? r.TestDate.Value.ToString("yyyy-MM-dd") : null, // Use conditional operator
                    pointsAwarded = r.PointsAwarded
                })
                .ToList();
            return Json(tests);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateApplicantRequirements()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
                _logger.LogInformation("Request body: {RequestBody}", body);
            }

            List<ApplicantRequirement> tests;
            try
            {
                tests = JsonConvert.DeserializeObject<List<ApplicantRequirement>>(body);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize test requirements.");
                return BadRequest("Invalid request body format.");
            }

            if (tests == null)
            {
                return BadRequest("Test data is null.");
            }

            // If no test records are provided, do nothing and return success.
            if (!tests.Any())
            {
                return Json(new { success = true });
            }

            foreach (var model in tests)
            {
                // Enforce valid score ranges:
                if (model.TestName == "ACT" && (model.Score < 0 || model.Score > 36))
                {
                    return BadRequest("ACT score must be between 0 and 36.");
                }
                if (model.TestName == "HESI" && (model.Score < 0 || model.Score > 100))
                {
                    return BadRequest("HESI score must be between 0 and 100.");
                }

                // Process update or insertion:
                if (model.RequirementId == 0)
                {
                    // New record – add it.
                    _context.ApplicantRequirements.Add(new ApplicantRequirement
                    {
                        ApplicantId = model.ApplicantId,
                        TestName = model.TestName,
                        Score = model.Score,
                        TestDate = model.TestDate
                    });
                }
                else
                {
                    // Existing record – update it.
                    var existingTest = _context.ApplicantRequirements
                        .FirstOrDefault(r => r.ApplicantId == model.ApplicantId && r.RequirementId == model.RequirementId);
                    if (existingTest != null)
                    {
                        existingTest.Score = model.Score;
                        existingTest.TestDate = model.TestDate;
                    }
                    else
                    {
                        // Fallback: if not found, add as new.
                        _context.ApplicantRequirements.Add(new ApplicantRequirement
                        {
                            ApplicantId = model.ApplicantId,
                            TestName = model.TestName,
                            Score = model.Score,
                            TestDate = model.TestDate
                        });
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Test requirements updated successfully.");
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update test requirements.");
                return StatusCode(500, "Internal server error while updating tests.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateApplicantExtraCredits([FromBody] UpdateExtraCreditsRequest request)
        {
            var applicant = _context.Applicants.FirstOrDefault(a => a.ApplicantId == request.ApplicantId);
            if (applicant == null)
            {
                return NotFound();
            }

            applicant.ExtraCredits = request.ExtraCredits;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update extra credits for applicant {ApplicantId}", request.ApplicantId);
                return StatusCode(500, "Internal error while updating extra credits.");
            }
        }

        [HttpPost]
        public IActionResult UpdateApplicantGPA([FromBody] int applicantId)
        {
            // Retrieve the applicant along with their courses.
            var applicant = _context.Applicants
                .Include(a => a.ApplicantCourses)
                .FirstOrDefault(a => a.ApplicantId == applicantId);
            if (applicant == null)
            {
                return Json(new { success = false, message = "Applicant not found." });
            }

            // Filter for courses that have a grade (even an F) and a completion date within the last 5 years.
            var validCourses = applicant.ApplicantCourses
                .Where(c => !string.IsNullOrEmpty(c.Grade)
                            && c.CompletionDate.HasValue
                            && c.CompletionDate.Value.AddYears(5) >= DateTime.Now);

            // Define the grade mapping.
            var gradeMapping = new Dictionary<string, decimal>
            {
                { "A", 4.0m },
                { "B", 3.0m },
                { "C", 2.0m },
                { "D", 1.0m },
                { "F", 0.0m }
            };

            // Only include courses with a valid grade that is in our mapping.
            var coursesForGPA = validCourses.Where(c => gradeMapping.ContainsKey(c.Grade));

            // Calculate average GPA; if no courses qualify, default to 0.
            decimal avgGPA = coursesForGPA.Any() ? coursesForGPA.Average(c => gradeMapping[c.Grade]) : 0;

            // Update the applicant's NursingGPA.
            applicant.NursingGPA = avgGPA;
            _context.SaveChanges();

            return Json(new { success = true, nursingGPA = avgGPA });
        }

        [AllowAnonymous]
        public IActionResult ModalAndTabReference()
        {
            _logger.LogInformation("ModalAndTabReference action called.");
            return View();
        }
    }
}

