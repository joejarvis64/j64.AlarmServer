using j64.AlarmServer.Web.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace j64.AlarmServer.Web.Data
{
    public class UserDataInitializer
    {
        private ApplicationDbContext _ctx;
        private UserManager<ApplicationUser> _userManager;

        public UserDataInitializer(ApplicationDbContext ctx, UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        public async Task CreateMasterUser()
        {
            var user = await _userManager.FindByNameAsync("admin");

            if (user == null)
            {
                user = new ApplicationUser()
                {
                    Email = "changeme@changeme.com",
                    UserName = "admin"
                };

                IdentityResult result = await _userManager.CreateAsync(user, "Admin_01");

                if (!result.Succeeded)
                {
                    MyLogger.LogError("Could not create admin user.  Messages were: " + String.Join("; ", result.Errors.Select(x => x.Description)));
                    return;
                }
            }

            var claims = await _userManager.GetClaimsAsync(user);
            await AddClaim(user, claims, "ManageConfig");
            await AddClaim(user, claims, "ArmDisarm");

            MyLogger.LogInfo("Added the default roles to admin user with default password and roles");
        }

        private async Task AddClaim(ApplicationUser user, IList<Claim> claims, string claim)
        {
            var roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

            if (claims.SingleOrDefault(x => x.Type == roleType && x.Value == claim) == null)
                await _userManager.AddClaimAsync(user, new Claim(roleType, claim, null));
        }
    }
}
