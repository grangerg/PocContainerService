using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PocContainerService;

static partial class Program
{
	
	public static async Task Main(string[] args)
	{
		WebApplicationBuilder builder;
		builder = WebApplication.CreateBuilder(args);
		builder.Services.Configure<JsonOptions>(options =>
		{
			// It's irritating that you can't REPLACE the object with one you constructed elsewhere (makes it hard to be DRY in larger projects).
			var standardOptions = options.SerializerOptions;
			standardOptions.AllowTrailingCommas = false; // Don't accommodate sloppiness
			standardOptions.DictionaryKeyPolicy = null; // Don't touch names when they're a dictionary in C# (in Javascript/Json, it could go either way).
			standardOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // deprecated: IgnoreNullValues = false
			standardOptions.IgnoreReadOnlyProperties = false;
			standardOptions.PropertyNameCaseInsensitive = false;
			standardOptions.PropertyNamingPolicy = null; // Follow the C# classes.
			standardOptions.ReadCommentHandling = JsonCommentHandling.Skip;
			standardOptions.WriteIndented = false;
			standardOptions.Converters.Add(new JsonStringEnumConverter()); // (de)serialize enum-members as strings, not numbers; preserve C# casing.
		});


		//		Yeah, yeah. I did it "long-hand" so I didn't have unused usings when I commented the below block out. :)
		//		diSvcs.AddHostedService<BackgroundService>(); is a MUCH shorter equivalent.
		//
		//builder.Host
		//	.ConfigureServices((ctx, diSvcs) =>
		//	{
		//		// E.g.; this doesn't actually work; need a "real" BackgroundService class...
		//		diSvcs.Add(new(typeof(Microsoft.Extensions.Hosting.IHostedService), typeof(Microsoft.Extensions.Hosting.BackgroundService), Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)); 
		//	});


		WebApplication app = builder.Build();
		// 
		// https://benfoster.io/blog/mvc-to-minimal-apis-aspnet-6/
		// The new "Minimal API" stuff is nifty, but for a "real" project, I feel lambdas in this location gets messy/confusing extremely quickly.
		// I still prefer keeping API/endpoint methods out of the main Program file, so the compromise is a partial class.
		// 
		app.MapGet("/", RootPath);
		app.MapGet($"/{nameof(Ready)}", Ready);
		app.MapGet($"/{nameof(Pause)}", Pause);
		app.MapGet($"/{nameof(Resume)}", Resume);
		app.MapGet($"/{nameof(Shutdown)}", Shutdown);
		app.MapGet($"/{nameof(Info)}", Info);
		app.Map("{*url}", DefaultPath);

		await app.RunAsync();
	}


}
