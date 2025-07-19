using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CanvasCode.Services;

public class CacheEvictionService(ICacheManager cacheManager, TimeSpan interval) : IHostedService, IDisposable {
	private readonly ICacheManager cacheManager = cacheManager;
	private readonly TimeSpan checkInterval = interval;
	private Timer? timer;
	
	public Task StartAsync(CancellationToken cancellationToken) {
		timer = new Timer(DoWork, null, TimeSpan.Zero, checkInterval);
		
		return Task.CompletedTask;
	}

	private void DoWork(object? state) {
		cacheManager.Prune();
	}
	
	public Task StopAsync(CancellationToken cancellationToken) {
		timer?.Change(Timeout.Infinite, 0);
		
		return Task.CompletedTask;
	}
	
	public void Dispose() {
		timer?.Dispose();
	}
}