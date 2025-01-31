using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nursing_Ranker.Data;

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

    public IActionResult Index()
    {
        _logger.LogInformation("Index action called.");

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
        return View("Dashboard"); // Explicitly specify the view name
    }

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
        return View();
    }
    public IActionResult ModalAndTabReference()
    {
        _logger.LogInformation("ModalAndTabReference action called.");
        return View();
    }
}



