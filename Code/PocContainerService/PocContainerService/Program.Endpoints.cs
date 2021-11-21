
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocContainerService;

static partial class Program
{
    static async Task RootPath(HttpContext ctx, [FromServices] EndpointDataSource endpointDataSource)
	{
		// The ONLY reason this isn't this     => "Up";     is because I wanted a list of convenience links, and I'm too "lazy" to find the "proper" way.
		StringBuilder sb = new();

		sb.AppendLine($"<html><head><title>{nameof(PocContainerService)}</title></head>")
			.AppendLine("<body>");

		sb.AppendLine("<h1>Status: Up</h1>");
		sb.AppendLine("<ol>");


		var endpoints = endpointDataSource.Endpoints
			.OfType<RouteEndpoint>()
			.Select(e => e.RoutePattern.RawText)
			.Where(url =>
			{
				if (string.IsNullOrEmpty(url))
					return false;
				return !(url == "/" || url.StartsWith("{")); // omit this route and the "default" route
			});
		foreach (var url in endpoints)
			sb.AppendLine($@"<li><a href=""{url}"">{url}</a></li>");

		sb.AppendLine("</ol>");

		sb.AppendLine("</body>")
			.AppendLine("</html>");

		ctx.Response.ContentType = "text/html";
		await ctx.Response.WriteAsync(sb.ToString());
	}



    static async Task DefaultPath(HttpContext ctx)
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        await ctx.Response.WriteAsync("You keep using that Url. I do not think it leads where you think it leads.");
    }



    static async Task Ready(HttpContext ctx, [FromServices] FibonacciService svc)
    {
		if (svc.Paused)
		{
			ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
			await ctx.Response.WriteAsync("Paused");
		}
		else
		{
			ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
			await ctx.Response.WriteAsync("Ready");
		}
	}



    static string Pause([FromServices] FibonacciService svc)
    {
		svc.Pause();
        return "Paused"; 
    }



    static string Resume([FromServices] FibonacciService svc)
    {
		svc.Resume();
        return "Resumed";
    }



    static string Shutdown([FromServices] FibonacciService svc)
    {
		svc.Shutdown();
        return "Shutdown"; // signal the worker process to stop as well as the web-host, so the entire app exits.
    }


    static IResult Info([FromServices] FibonacciService svc)
    {
        return Results.Ok(new { FibonnaciNumber = svc.FibonacciNumber, Paused = svc.Paused }); // emit Json info about the work being performed
	}


}
