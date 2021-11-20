
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PocContainerService;

static partial class Program
{
    static string RootPath()
        => "Up";

    static async Task DefaultPath(HttpContext ctx)
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        await ctx.Response.WriteAsync("You keep using that Url. I do not think it leads where you think it leads.");
    }


    static string Ready()
    {
        return "Running/Paused"; // this should reflect whether the service is paused or running.
    }

    static string Pause()
    {
        return "Paused"; 
    }


    static string Resume()
    {
        return "Resumed";
    }


    static string Shutdown()
    {
        return "Shutdown"; // signal the worker process to stop as well as the web-host, so the entire app exits.
    }


    static IResult Info()
    {
        return Results.Ok(new { Message = "Info and stuff" }); // emit Json info about the work being performed
	}


}
