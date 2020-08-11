using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace BlazorServerHost.Services
{
	public class StateService : IDisposable, INotifyPropertyChanged
	{
		private readonly Guid _instanceId = Guid.NewGuid();

		private readonly ILogger<StateService> _logger;

		public StateService(ILogger<StateService> logger)
		{
			_logger = logger;
			_logger.LogInformation("State service instantiated with id {InstanceId}", _instanceId);
		}

		public Dictionary<string, object> State{ get; private set; } = new Dictionary<string, object>();

		public T GetState<T>(string name, T defaultValue)
		{
			if (!State.ContainsKey(name))
			{
				SetState(name, defaultValue);
			}

			return (T)State[name];
		}

		public void SetState<T>(string name, T value)
		{
			_logger.LogInformation("State service {InstanceId} storing {Name} with {Value}", _instanceId, name, value);

			if (!State.TryGetValue(name, out var currentValue) || !currentValue.Equals(value))
			{
				State[name] = value;
				OnPropertyChanged(nameof(State));
			}
		}

		public void Dispose()
		{
			_logger.LogInformation("State service {InstanceId} disposing", _instanceId);
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class GlobalStateService : StateService
	{
		public GlobalStateService(ILogger<GlobalStateService> logger)
			: base (logger)
		{
		}
	}
	public class UserStateService : StateService
	{
		public UserStateService(ILogger<UserStateService> logger)
			: base(logger)
		{
		}
	}

	public class ScopedStateService : StateService
	{
		public ScopedStateService(ILogger<ScopedStateService> logger)
			: base(logger)
		{
		}
	}

	public class SessionStateService : StateService
	{
		public SessionStateService(ILogger<GlobalStateService> logger)
			: base(logger)
		{
		}
	}
}
