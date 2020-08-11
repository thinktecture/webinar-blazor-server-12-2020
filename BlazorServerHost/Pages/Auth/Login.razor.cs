using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BlazorServerHost.Services;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorServerHost.Pages.Auth
{
	public partial class Login : ComponentBase
	{
		[Inject] private IdentityAuthenticationStateProvider _authStateProvider { get; set; }
		[Inject] private IJSRuntime _js { get; set; }
		[Inject] private NavigationManager _navigationManager { get; set; }
		[Inject] private IMatToaster _matToaster { get; set; }

		private LoginDto _loginParameters { get; set; } = new LoginDto();

		[CascadingParameter]
		public Task<AuthenticationState> AuthenticationStateTask { get; set; }

		[Parameter]
		public string ReturnUrl { get; set; }

		protected override async Task OnParametersSetAsync()
		{
			var user = (await AuthenticationStateTask).User;

			if (user.Identity.IsAuthenticated)
			{
				_navigationManager.NavigateTo(_navigationManager.BaseUri + ReturnUrl);
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
				await _js.InvokeVoidAsync("SetFocus", "userName");

			await base.OnAfterRenderAsync(firstRender);
		}

		private void Register()
		{
			_navigationManager.NavigateTo("auth/register");
		}

		private async Task SubmitLogin()
		{
			try
			{
				var success = await _authStateProvider.LoginAsync(_loginParameters.UserName, _loginParameters.Password);
				if (success)
				{
					// On successful Login the response.Message is the Last Page Visited from User Profile
					// We can't navigate yet as the setup is proceeding asynchronously
					//if (!string.IsNullOrEmpty(response.Message))
					//{
					//	_navigateTo = response.Message;
					//}
					//else
					//{
					//	_navigateTo = "/dashboard";
					//}

					_navigationManager.NavigateTo(_navigationManager.BaseUri + ReturnUrl);
				}
				else
				{
					_matToaster.Add("Login failed", MatToastType.Danger, "Login Attempt Failed");
				}
			}
			catch (Exception ex)
			{
				_matToaster.Add(ex.Message, MatToastType.Danger, "Login Attempt Failed");
			}
		}

		public class LoginDto
		{
			[Required]
			public string UserName { get; set; }
			[Required]
			public string Password { get; set; }
			public bool RememberMe { get; set; }
		}
	}
}
