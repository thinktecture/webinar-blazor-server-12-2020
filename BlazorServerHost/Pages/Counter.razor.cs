using System;
using System.ComponentModel;
using System.Threading.Tasks;
using BlazorServerHost.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BlazorServerHost.Pages
{
	public partial class Counter : ComponentBase, IDisposable
	{
		private string _userName;

		[Inject]
		private GlobalStateService _global { get; set; }

		[Inject]
		private ScopedStateService _scoped { get; set; }

		[Inject]
		private AuthenticationStateProvider _authState { get; set; }

		[Inject]
		private CircuitStore _circuitStore { get; set; }

		protected override void OnInitialized()
		{
			base.OnInitialized();

			_global.PropertyChanged += OnPropertyChanged;
			_scoped.PropertyChanged += OnPropertyChanged;
		}

		private async void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			await Update();
		}

		protected override async Task OnInitializedAsync()
		{
			var state = await _authState.GetAuthenticationStateAsync();
			_userName = state.User?.Identity?.Name;

			await LoadSessionCountAsync();
		}

		private async Task Update()
		{
			await LoadSessionCountAsync();
			await InvokeAsync(StateHasChanged);
		}

		private int globalCount => _global.GetState<int>(nameof(globalCount), 0);

		private void IncrementGlobalCount()
		{
			_global.SetState(nameof(globalCount), globalCount + 1);
		}


		private int userCount => _global.GetState<int>(_userName, 0);

		private void IncrementUserCount()
		{
			_global.SetState(_userName, userCount + 1);
		}


		private int scopedCount => _scoped.GetState<int>(nameof(scopedCount), 0);

		private void IncrementScopedCount()
		{
			_scoped.SetState(nameof(scopedCount), scopedCount + 1);
		}

		private int sessionCount;

		private async Task IncrementSessionCount()
		{
			await LoadSessionCountAsync();
			sessionCount++;
			await _circuitStore.SaveValueAsync("sessionCounter", sessionCount.ToString());
		}

		private async Task LoadSessionCountAsync()
		{
			var sessionCounter = await _circuitStore.LoadValueAsync("sessionCounter");
			int.TryParse(sessionCounter, out sessionCount);
		}


		private int componentCount = 0;

		private void IncrementComponentCount()
		{
			componentCount++;
		}

		public void Dispose()
		{
			_global.PropertyChanged -= OnPropertyChanged;
			_scoped.PropertyChanged -= OnPropertyChanged;
		}
	}
}
