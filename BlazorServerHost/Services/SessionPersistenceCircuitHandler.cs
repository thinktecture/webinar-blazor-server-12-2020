using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorServerHost.Services
{
	public class SessionPersistenceCircuitHandler : CircuitHandler
	{
		private readonly Guid _instanceId = Guid.NewGuid();
		private readonly ILogger<SessionPersistenceCircuitHandler> _logger;
		private readonly IJSRuntime _jsRuntime;
		private readonly CircuitStore _circuitStore;

		public SessionPersistenceCircuitHandler(ILogger<SessionPersistenceCircuitHandler> logger, IJSRuntime jsRuntime, CircuitStore circuitStore)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
			_circuitStore = circuitStore ?? throw new ArgumentNullException(nameof(circuitStore));

			_logger.LogInformation("Circuit Handler created with Id {CircuitHandlerId}", _instanceId);
		}

		public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_logger.LogInformation("CircuitHandler {CircuitHandlerId} got {CircuitEvent} for Circuit {CircuitId}", _instanceId, "OPENED", circuit.Id);

			var remoteSessionId = await GetRemoteSessionIdAsync(cancellationToken);
			if (!String.IsNullOrEmpty(remoteSessionId))
			{
				_logger.LogInformation("CircuitHandler {CircuitHandlerId} FOUND remote session {RemoteSessionId} for Circuit {CircuitId}", _instanceId, remoteSessionId, circuit.Id);
			}
			else
			{
				remoteSessionId = Guid.NewGuid().ToString();
				await SetRemoteSessionIdAsync(remoteSessionId, cancellationToken);
				_logger.LogInformation("CircuitHandler {CircuitHandlerId} CREATED {RemoteSessionId} for Circuit {CircuitId}", _instanceId, remoteSessionId, circuit.Id);
			}

			_circuitStore.InitializeCircuitId(circuit.Id, remoteSessionId);
			await _circuitStore.SaveValueAsync($"LastAccessed", DateTime.UtcNow.ToString("g"), cancellationToken);

			await base.OnCircuitOpenedAsync(circuit, cancellationToken);
		}

		public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_logger.LogInformation("CircuitHandler {CircuitHandlerId} got {CircuitEvent} for Circuit {CircuitId}", _instanceId, "connection down", circuit.Id);

			return base.OnConnectionDownAsync(circuit, cancellationToken);
		}

		public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_logger.LogInformation("CircuitHandler {CircuitHandlerId} got {CircuitEvent} for Circuit {CircuitId}", _instanceId, "connection up", circuit.Id);

			return base.OnConnectionUpAsync(circuit, cancellationToken);
		}

		public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
		{
			_logger.LogInformation("CircuitHandler {CircuitHandlerId} got {CircuitEvent} for Circuit {CircuitId}", _instanceId, "CLOSED", circuit.Id);

			await _circuitStore.SaveValueAsync($"LastAccessed", DateTime.UtcNow.ToString("g"), cancellationToken);
			await base.OnCircuitClosedAsync(circuit, cancellationToken);
		}

		#region JS Interop

		private const string RemoteSessionIdKey = "sessionId";

		private async Task<string> GetRemoteSessionIdAsync(CancellationToken cancellationToken = default)
		{
			return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", cancellationToken, RemoteSessionIdKey);
		}

		private async Task SetRemoteSessionIdAsync(string value, CancellationToken cancellationToken = default)
		{
			await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", cancellationToken, RemoteSessionIdKey, value);
		}

		#endregion
	}
}
