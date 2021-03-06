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
	public ulong FibonacciNumber { get; private set; }
	public ulong PrevNumber { get; private set; }


	public FibonacciService(ILogger<FibonacciService> logger, IHostApplicationLifetime appLifetime, IConfiguration appConfig)
	{
		_logger = logger;
		_appLifetime = appLifetime;
		_appSettings = appConfig.GetSection("AppSettings");

		PrevNumber = 0;
		FibonacciNumber = 1;
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
					ulong _tmp = FibonacciNumber;
					FibonacciNumber = checked(FibonacciNumber + PrevNumber); // we want OverflowException instead of the default bit-truncation behavior.
					PrevNumber = _tmp;
					_logger.LogInformation($"New Fibonacci number: {FibonacciNumber:N0}; Old number: {PrevNumber:N0}");
				}
				catch (System.OverflowException)
				{
					_logger.LogInformation("Fibonacci number too large for a ulong. Starting over.");
					PrevNumber = 0;
					FibonacciNumber = 1;
				}
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
