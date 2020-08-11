using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace BlazorServerHost.Pages.Auth
{
	public partial class Register : ComponentBase
	{
		[Inject] private IJSRuntime _js { get; set; }
		[Inject] private IMatToaster _matToaster { get; set; }
		[Inject] private UserManager<IdentityUser> _userManager { get; set; }
		[Inject] private NavigationManager _navigationManager { get; set; }

		private RegisterDto _registerModel { get; set; } = new RegisterDto();

		[CascadingParameter]
		public Task<AuthenticationState> AuthenticationStateTask { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
				await _js.InvokeVoidAsync("SetFocus", "userName");

			await base.OnAfterRenderAsync(firstRender);
		}

		private async Task SubmitRegister()
		{
			try
			{
				var user = await _userManager.FindByEmailAsync(_registerModel.Email)
							?? await _userManager.FindByNameAsync(_registerModel.UserName);
				if (user != null)
				{
					_matToaster.Add("Username or email already exists.", MatToastType.Danger, "Registration Attempt Failed");
					return;
				}

				user = new IdentityUser(_registerModel.UserName)
				{
					Email = _registerModel.Email,
					EmailConfirmed = true,
				};
				var result = await _userManager.CreateAsync(user);
				if (!result.Succeeded)
				{
					foreach (var err in result.Errors)
					{
						_matToaster.Add(err.Description, MatToastType.Warning, "Registration Attempt Failed");
					}

					return;
				}

				result = await _userManager.AddPasswordAsync(user, _registerModel.Password);
				if (!result.Succeeded)
				{
					await _userManager.DeleteAsync(user);

					foreach (var err in result.Errors)
					{
						_matToaster.Add(err.Description, MatToastType.Warning, "Registration Attempt Failed");
					
					}
					return;
				}

				_matToaster.Add("Registered successfully, please sign in.", MatToastType.Info, "Registration attempt succeeded");
				_navigationManager.NavigateTo(_navigationManager.BaseUri + "auth/login");
			}
			catch (Exception ex)
			{
				_matToaster.Add(ex.Message, MatToastType.Danger, "Registration Attempt Failed");
			}
		}

		public class RegisterDto
		{
			[Required]
			public string UserName { get; set; }

			[Required, EmailAddress]
			public string Email { get; set; }

			[Required]
			public string Password { get; set; }

			[Required, SameAs(nameof(Password))]
			public string PasswordRepeat { get; set; }
		}

		public class SameAsAttribute : ValidationAttribute
		{
			private readonly string _comparisonProperty;

			public SameAsAttribute(string comparisonProperty)
			{
				_comparisonProperty = comparisonProperty;
			}

			protected override ValidationResult IsValid(object value, ValidationContext validationContext)
			{
				ErrorMessage = $"{validationContext.DisplayName} needs to be the same as {_comparisonProperty}.";
				var currentValue = value;

				var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

				if (property == null)
					throw new ArgumentException("Property with this name not found");

				var comparisonValue = property.GetValue(validationContext.ObjectInstance);

				if (!currentValue.Equals(comparisonValue))
					return new ValidationResult(ErrorMessage);

				return ValidationResult.Success;
			}
		}
	}
}
