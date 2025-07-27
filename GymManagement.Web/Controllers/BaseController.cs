using GymManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GymManagement.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IUserSessionService _userSessionService;
        protected readonly ILogger _logger;

        protected BaseController(IUserSessionService userSessionService, ILogger logger)
        {
            _userSessionService = userSessionService;
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                // Check if user is authenticated and session is valid
                if (User.Identity?.IsAuthenticated == true)
                {
                    var currentUser = await _userSessionService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        _logger.LogWarning("User session not found for authenticated user: {Username}", 
                            _userSessionService.GetUserName());
                        
                        // Try to rebuild session
                        await _userSessionService.RefreshUserClaimsAsync();
                        currentUser = await _userSessionService.GetCurrentUserAsync();
                        
                        if (currentUser == null)
                        {
                            _logger.LogError("Failed to rebuild user session for: {Username}", 
                                _userSessionService.GetUserName());
                            
                            // Redirect to login if session cannot be rebuilt
                            context.Result = RedirectToAction("Login", "Auth");
                            return;
                        }
                    }

                    // Check if user account is still active
                    if (!currentUser.KichHoat)
                    {
                        _logger.LogWarning("Inactive user attempted to access: {Username}", currentUser.TenDangNhap);
                        await _userSessionService.ClearCurrentUserAsync();
                        context.Result = RedirectToAction("Login", "Auth", new { message = "Tài khoản của bạn đã bị vô hiệu hóa." });
                        return;
                    }

                    // Make current user available to derived controllers
                    ViewBag.CurrentUser = currentUser;
                }

                await base.OnActionExecutionAsync(context, next);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BaseController.OnActionExecutionAsync");
                
                // Clear potentially corrupted session
                await _userSessionService.ClearCurrentUserAsync();
                
                // Redirect to error page or login
                if (User.Identity?.IsAuthenticated == true)
                {
                    context.Result = RedirectToAction("Login", "Auth", new { message = "Đã xảy ra lỗi với phiên đăng nhập. Vui lòng đăng nhập lại." });
                }
                else
                {
                    await base.OnActionExecutionAsync(context, next);
                }
            }
        }

        /// <summary>
        /// Get current user safely with error handling
        /// </summary>
        protected async Task<UserSessionInfo?> GetCurrentUserSafeAsync()
        {
            try
            {
                return await _userSessionService.GetCurrentUserAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user safely");
                return null;
            }
        }

        /// <summary>
        /// Get current user ID safely
        /// </summary>
        protected string? GetCurrentUserIdSafe()
        {
            try
            {
                return _userSessionService.GetUserId();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID safely");
                return null;
            }
        }

        /// <summary>
        /// Get current NguoiDungId safely
        /// </summary>
        protected int? GetCurrentNguoiDungIdSafe()
        {
            try
            {
                return _userSessionService.GetNguoiDungId();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current NguoiDungId safely");
                return null;
            }
        }

        /// <summary>
        /// Check if current user has specific role safely
        /// </summary>
        protected bool IsInRoleSafe(string role)
        {
            try
            {
                return _userSessionService.IsInRole(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user role safely: {Role}", role);
                return false;
            }
        }

        /// <summary>
        /// Handle user not found scenario
        /// </summary>
        protected IActionResult HandleUserNotFound(string? action = null)
        {
            _logger.LogWarning("User not found - redirecting to login");
            TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.";
            
            if (!string.IsNullOrEmpty(action))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(action) });
            }
            
            return RedirectToAction("Login", "Auth");
        }

        /// <summary>
        /// Handle unauthorized access
        /// </summary>
        protected IActionResult HandleUnauthorized(string message = "Bạn không có quyền truy cập chức năng này.")
        {
            _logger.LogWarning("Unauthorized access attempt by user: {Username}", _userSessionService.GetUserName());
            TempData["ErrorMessage"] = message;
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Handle general errors with logging
        /// </summary>
        protected IActionResult HandleError(Exception ex, string userMessage = "Đã xảy ra lỗi. Vui lòng thử lại sau.")
        {
            _logger.LogError(ex, "Error handled in controller: {Controller}", GetType().Name);
            TempData["ErrorMessage"] = userMessage;
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Log user action for audit trail
        /// </summary>
        protected void LogUserAction(string action, object? data = null)
        {
            try
            {
                var username = _userSessionService.GetUserName();
                var userId = _userSessionService.GetUserId();
                
                if (data != null)
                {
                    _logger.LogInformation("User action: {Username} ({UserId}) performed {Action} with data: {@Data}", 
                        username, userId, action, data);
                }
                else
                {
                    _logger.LogInformation("User action: {Username} ({UserId}) performed {Action}", 
                        username, userId, action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user action: {Action}", action);
            }
        }
    }
}
