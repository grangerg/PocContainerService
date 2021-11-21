using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace PocContainerService;

public class FibonacciService : BackgroundService
{
	readonly ILogger<FibonacciService> _logger;
	readonly IHostApplicationLifetime _appLifetime; 
	readonly IConfigurationSection _appSettings;

	public bool Paused { get; private set; }
	public long FibonacciNumber { get; private set; }
	long _num1, _num2;

	public FibonacciService(ILogger<FibonacciService> logger, IHostApplicationLifetime appLifetime, IConfiguration appConfig)
	{
		_logger = logger;
		_appLifetime = appLifetime;
		_appSettings = appConfig.GetSection("AppSettings");

		_num1 = _num2 = 1;
		FibonacciNumber = 0;
	}


	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		const int defaultDelayMs = 2000;
		while (!stoppingToken.IsCancellationRequested)
		{
			if (!Paused)
			{
				try
				{
					FibonacciNumber = _num1 + _num2;
				}
				catch
				{
					_logger.LogInformation("Fibonacci number too large. Starting over.");
					_num1 = _num2 = 1;
					FibonacciNumber = _num1 + _num2;
				}
				_num2 = _num1;
				_num1 = FibonacciNumber;
				_logger.LogInformation($"New Fibonacci number: {FibonacciNumber:N0}");
			}
			
			await Task.Delay(_appSettings.GetValue("DelayMs", defaultDelayMs), stoppingToken);
		}
	}


	public void Pause()
		=> Paused = true;

	public void Resume()
		=> Paused = false;

	public void Shutdown()
		=> _appLifetime.StopApplication();


}
