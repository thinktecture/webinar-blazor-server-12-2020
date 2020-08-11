using System.Linq;
using System.Threading.Tasks;
using MatBlazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerHost.Pages
{
	[Authorize]
	public partial class UserManagement : ComponentBase
	{
	
		[Inject]
		private UserManager<IdentityUser> _userManager { get; set; }

		private User[] users { get; set; }
		private int userCount = 0;
		private int pageSize = 5;
		private int pageIndex = 0;

		protected override async Task OnInitializedAsync()
		{
			await LoadUsersPaged();
		}

		private async Task OnPageAsync(MatPaginatorPageEvent e)
		{
			pageSize = e.PageSize;
			pageIndex = e.PageIndex;

			await LoadUsersPaged();
		}

		private async Task LoadUsersPaged()
		{
			userCount = await _userManager.Users.CountAsync();
			users = await _userManager.Users
				.OrderBy(u => u.NormalizedUserName)
				.Skip(pageIndex * pageSize)
				.Take(pageSize)
				.Select(u => new User(u))
				.ToArrayAsync();

			StateHasChanged();
		}
	}

	public class User
	{
		public string Id { get; set; }
		public string Username { get; set; }
		public string EMail { get; set; }
		public bool EMailConfirmed { get; set; }

		public User() { }

		public User(IdentityUser user)
		{
			Id = user.Id;
			Username = user.UserName;
			EMail = user.Email;
			EMailConfirmed = user.EmailConfirmed;
		}
	}
}
