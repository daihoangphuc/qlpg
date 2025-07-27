using GymManagement.Web.Data.Models;
using GymManagement.Web.Models.ViewModels;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IUserSessionService userSessionService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.AuthenticateAsync(model.Username, model.Password);

            if (user != null)
            {
                try
                {
                    var principal = await _authService.CreateClaimsPrincipalAsync(user);
                    await HttpContext.SignInAsync("Cookies", principal);

                    // Set user session
                    await _userSessionService.SetCurrentUserAsync(user);

                    _logger.LogInformation("User {Username} logged in successfully.", model.Username);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login process for user {Username}", model.Username);
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.");
                    return View(model);
                }
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new TaiKhoan
            {
                TenDangNhap = model.Username,
                Email = model.Email,
                KichHoat = true
            };

            var result = await _authService.CreateUserAsync(user, model.Password);

            if (result)
            {
                try
                {
                    _logger.LogInformation("User {Username} created a new account with password.", model.Username);

                    // Sign in the user
                    var createdUser = await _authService.GetUserByUsernameAsync(model.Username);
                    if (createdUser != null)
                    {
                        var principal = await _authService.CreateClaimsPrincipalAsync(createdUser);
                        await HttpContext.SignInAsync("Cookies", principal);

                        // Set user session
                        await _userSessionService.SetCurrentUserAsync(createdUser);
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during registration process for user {Username}", model.Username);
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại.");
                    return View(model);
                }
            }

            ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản. Tên đăng nhập hoặc email có thể đã tồn tại.");

            return View(model);
        }

        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var username = _userSessionService.GetUserName();

                // Clear user session
                await _userSessionService.ClearCurrentUserAsync();

                // Sign out from authentication
                await HttpContext.SignOutAsync("Cookies");

                _logger.LogInformation("User {Username} logged out.", username);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout process");
                // Still try to sign out even if there's an error
                await HttpContext.SignOutAsync("Cookies");
                return RedirectToAction("Index", "Home");
            }
        }



        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email là bắt buộc.");
                return View();
            }

            // TODO: Implement forgot password logic
            // For now, just show success message
            TempData["SuccessMessage"] = "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi link đặt lại mật khẩu.";

            return RedirectToAction("Login");
        }

    }
}
