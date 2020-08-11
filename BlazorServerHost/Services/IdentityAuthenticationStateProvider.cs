using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BlazorServerHost.Services
{
	public class IdentityAuthenticationStateProvider :
		RevalidatingIdentityAuthenticationStateProvider<IdentityUser>
	{
		private ClaimsIdentity _currentUser = new ClaimsIdentity();

		public IdentityAuthenticationStateProvider(
			ILoggerFactory loggerFactory,
			IServiceScopeFactory scopeFactory,
			IOptions<IdentityOptions> optionsAccessor)
			: base(loggerFactory, scopeFactory, optionsAccessor)
		{
		}

		public virtual async Task<bool> LoginAsync(string userId, string password)
		{
			// Log out current user
			_currentUser = new ClaimsIdentity();
			bool success = false;

			// Try to validate new user
			using var scope = _scopeFactory.CreateScope();
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

			var user = await userManager.FindByNameAsync(userId);

			if (user != null && await userManager.CheckPasswordAsync(user, password))
			{
				success = true;

				_currentUser = await BuildClaimsIdentity(userManager, user);
			}

			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
			return success;
		}

		public virtual async Task<ClaimsIdentity> LogoutAsync()
		{
			_currentUser = new ClaimsIdentity();
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

			return _currentUser;
		}


		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			return new AuthenticationState(new ClaimsPrincipal(_currentUser));
		}

		private async Task<ClaimsIdentity> BuildClaimsIdentity(UserManager<IdentityUser> userManager, IdentityUser user)
		{
			var userClaims = await userManager.GetClaimsAsync(user);
			var roles = await userManager.GetRolesAsync(user);

			// Build claims identity
			var claims = new List<Claim>()
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
			};

			claims.AddRange(userClaims);
			claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

			return new ClaimsIdentity(claims,"local", JwtRegisteredClaimNames.Sub, ClaimTypes.Role);
		}
	}
}
