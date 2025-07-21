using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GymManagement.Web.Services;
using GymManagement.Web.Models.DTOs;

namespace GymManagement.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
            return View(new LoginDto { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                
                return RedirectToAction("Index", "Home");
            }

            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Đăng nhập thất bại");
            }

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

            return View(new RegisterDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Login));
            }

            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Đăng ký thất bại");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            TempData["SuccessMessage"] = "Đăng xuất thành công";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordDto());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ChangePasswordAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Profile");
            }

            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Đổi mật khẩu thất bại");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ForgotPasswordAsync(model.Email);
            
            // Always show success message for security reasons
            TempData["SuccessMessage"] = "Nếu email tồn tại, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? token = null, string? email = null)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Liên kết đặt lại mật khẩu không hợp lệ";
                return RedirectToAction(nameof(Login));
            }

            return View(new ResetPasswordDto { Token = token, Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ResetPasswordAsync(model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Login));
            }

            if (result.Errors.Any())
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Đặt lại mật khẩu thất bại");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(user);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
