using System;
using System.Threading;
using System.Threading.Tasks;
using BlazorServerHost.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorServerHost.Services
{
	public class CircuitStore
	{
		private readonly Guid _instanceId = Guid.NewGuid();
		private readonly ILogger<CircuitStore> _logger;
		private readonly IServiceProvider _serviceProvider;

		private string _circuitId;
		private string _sessionId;

		public bool HasCircuit => !String.IsNullOrEmpty(_circuitId);

		public CircuitStore(ILogger<CircuitStore> logger, IServiceProvider serviceProvider)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public void InitializeCircuitId(string circuitId, string sessionId)
		{
			_logger.LogInformation("CircuitStore {StoreInstanceId} initialized with CircuitId {CircuitId}", _instanceId, circuitId);

			_circuitId = circuitId;
			_sessionId = sessionId;
		}

		public async Task<string> LoadValueAsync(string key, CancellationToken cancellationToken = default)
		{
			if (!HasCircuit)
				return null;

			using var scope = _serviceProvider.CreateScope();
			await using var ctx = scope.ServiceProvider.GetService<ApplicationDbContext>();
			var entry = await ctx.Values.FirstOrDefaultAsync(v => v.Key == $"{_sessionId}_{key}", cancellationToken);
			
			_logger.LogInformation("CircuitStore {StoreInstanceId} found value {value} for key {key} on circuit {CircuitId}", _instanceId, entry?.Value, key, _circuitId);

			return entry?.Value;
		}

		public async Task SaveValueAsync(string key, string value, CancellationToken cancellationToken = default)
		{
			if (!HasCircuit)
				return;

			using var scope = _serviceProvider.CreateScope();
			await using var ctx = scope.ServiceProvider.GetService<ApplicationDbContext>();
			var entry = await ctx.Values.FirstOrDefaultAsync(v => v.Key == $"{_sessionId}_{key}", cancellationToken);

			if (entry == null)
			{
				entry = new KeyValue() { Key = $"{_sessionId}_{key}" };
				await ctx.Values.AddAsync(entry, cancellationToken);
			}

			entry.Value = value;

			_logger.LogInformation("CircuitStore {StoreInstanceId} saved value {value} for key {key} on circuit {CircuitId}", _instanceId, value, key, _circuitId);

			await ctx.SaveChangesAsync(cancellationToken);
		}
	}
}
