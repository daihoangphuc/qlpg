using Microsoft.AspNetCore.Identity;

namespace GymManagement.Web.Data.Identity
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
