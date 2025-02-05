using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Nursing_Ranker.Data;
using Nursing_Ranker.Models;
using Nursing_Ranker.Models.ClassModels;
using System.Security.Claims;


namespace Nursing_Ranker.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            _logger.LogInformation("Login GET action called. ReturnUrl: {ReturnUrl}", returnUrl);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            _logger.LogInformation("Login POST action called. ReturnUrl: {ReturnUrl}", returnUrl);

            if (ModelState.IsValid)
            {
                var user = _context.Users.SingleOrDefault(u => u.Email == model.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed. Email not registered: {Email}", model.Email);
                    ModelState.AddModelError("Email", "The email is not registered.");
                }
                else if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed. Incorrect password for email: {Email}", model.Email);
                    ModelState.AddModelError("Password", "The password is incorrect.");
                }
                else
                {
                    // Store user ID in session
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    _logger.LogInformation("User ID stored in session: {UserId}", user.Id);

                    // Set up authentication claims
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    _logger.LogInformation("User authenticated and signed in.");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        _logger.LogInformation("Redirecting to returnUrl: {ReturnUrl}", returnUrl);
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        _logger.LogInformation("Redirecting to Dashboard.");
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
            }

            // Log the model state errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("Model state error: {ErrorMessage}", error.ErrorMessage);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation("Register POST action called.");

            if (ModelState.IsValid)
            {
                // Check if the email is already used
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    _logger.LogWarning("Registration failed. Email already in use: {Email}", model.Email);
                    ModelState.AddModelError("Email", "The email is already in use.");
                    return View(model);
                }

                // Save the profile picture
                string uniqueFileName = null;
                if (model.ProfilePicture != null)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\profile_images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePicture.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(fileStream);
                    }
                }

                // Create a new User object
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), // Hash the password
                    ProfilePicturePath = uniqueFileName
                };

                // Save the user to the database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Store user ID in session
                HttpContext.Session.SetInt32("UserId", user.Id);
                _logger.LogInformation("User registered and ID stored in session: {UserId}", user.Id);

                // If successful, redirect to a different page
                return RedirectToAction("Dashboard", "Dashboard");
            }

            // If we got this far, something failed; redisplay the form
            return View(model);
        }

        // Get the user's profile to edit
        [HttpGet]
        public IActionResult EditProfile()
        {
            _logger.LogInformation("EditProfile GET action called.");

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            // Pass user data to the view
            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ExistingProfilePicture = user.ProfilePicturePath
            };

            return PartialView("~/Views/Dashboard/_EditProfile.cshtml", model);
        }

        // Post the changes to the user's profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            _logger.LogInformation("EditProfile POST action called.");

            if (!ModelState.IsValid)
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "User");
                }

                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound();
                }

                // Update fields to the new inputs on the modal
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;

                // Handle new profile picture upload if they did not have one
                if (model.ProfilePicture != null)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\profile_images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePicture.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(fileStream);
                    }

                    // Delete old profile picture if exists
                    if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                    {
                        var oldFilePath = Path.Combine(uploadsFolder, user.ProfilePicturePath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    user.ProfilePicturePath = uniqueFileName;
                }

                // Save changes
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Profile updated successfully!" });
            }

            return PartialView("~/Views/Dashboard/_EditProfile.cshtml", model); // Return validation errors
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Logout action called. Clearing session and signing out.");

            // Clear authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear session
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

    }
}