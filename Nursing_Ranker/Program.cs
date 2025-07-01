using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Nursing_Ranker.Data;
using Nursing_Ranker.Models.ClassModels;
using Nursing_Ranker.Models.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddSession(static options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add distributed memory cache
builder.Services.AddDistributedMemoryCache();

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(static options =>
    {
        options.LoginPath = "/User/Login";
    });

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Use session
app.UseSession();

app.UseEndpoints(static endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});


void SeedDemoData(ApplicationDbContext context)
{
    if (context.Users.Any(u => u.Email == "demo@demo.com"))
        return;

    // Create Demo User
    var demoUser = new User
    {
        FirstName = "Fred",
        LastName = "Smith",
        Email = "demo@demo.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
        FavColor = "blue"
    };
    context.Users.Add(demoUser);
    context.SaveChanges();

    var random = new Random();
    var firstNames = new[] { "Alice", "Bob", "Charlie", "Diana", "Evan", "Fiona", "George", "Hannah", "Ian", "Julia" };
    var lastNames = new[] { "Johnson", "Miller", "Davis", "Garcia", "Martinez", "Clark", "Lewis", "Hall", "Young", "Allen" };
    var grades = new[] { "A", "B", null };

    for (int i = 0; i < 10; i++)
    {
        var applicant = new Applicant
        {
            FirstName = firstNames[random.Next(firstNames.Length)],
            LastName = lastNames[random.Next(lastNames.Length)],
            WSCCGPA = (decimal)Math.Round(2.5 + random.NextDouble() * 1.5, 2), // GPA between 2.5 and 4.0
            ExtraCredits = random.Next(0, 3) // 0 to 2
        };
        context.Applicants.Add(applicant);
        context.SaveChanges();

        // Courses
        foreach (var course in CourseScoring.CourseRules.Keys)
        {
            string? grade = grades[random.Next(grades.Length)];
            DateTime? completionDate = grade != null ? DateTime.Now.AddDays(-random.Next(0, 5 * 365)) : null;

            context.ApplicantCourses.Add(new ApplicantCourse
            {
                ApplicantId = applicant.ApplicantId,
                CourseName = course,
                Grade = grade,
                Completed = grade != null,
                CompletionDate = completionDate,
                RequiresRetake = false,
                PointsAwarded = grade == "A" ? 500 : grade == "B" ? 250 : 0
            });
        }

        // Test Requirements (ACT and HESI)
        var testNames = new[] { "ACT", "HESI" };
        foreach (var test in testNames)
        {
            decimal score = test == "ACT"
                ? random.Next(15, 30) // 15–29 for ACT
                : random.Next(60, 100); // 60–99 for HESI

            var testDate = DateTime.Now.AddDays(-random.Next(0, 5 * 365));

            context.ApplicantRequirements.Add(new ApplicantRequirement
            {
                ApplicantId = applicant.ApplicantId,
                TestName = test,
                Score = score,
                TestDate = testDate
            });
        }

        context.SaveChanges();
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedDemoData(context);
}


app.Run();




