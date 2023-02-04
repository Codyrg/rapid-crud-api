namespace Api;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public static class Extensions
{   
    private static bool _useApiKeysInitialized = false;

    public static void MapAuthorizedGet(this WebApplication app, string route, Delegate handler)
    {
        app.UseApiKeys();
        app.MapGet(route, async ([FromHeader(Name = "X-API-KEY")] string apiKey, AuthorizerService authorizer) =>
        {
            await authorizer.Authorize(apiKey);
            return handler.DynamicInvoke();
        });
    }

    public static void MapAuthorizedPost(this WebApplication app, string route, Delegate handler)
    {
        app.UseApiKeys();
        app.MapPost(route, async ([FromHeader(Name = "X-API-KEY")] string apiKey, AuthorizerService authorizer) =>
        {
            await authorizer.Authorize(apiKey);
            return handler.DynamicInvoke();
        });
    }

    public static void MapAuthorizedPut(this WebApplication app, string route, Delegate handler)
    {
        app.UseApiKeys();
        app.MapPut(route, async ([FromHeader(Name = "X-API-KEY")] string apiKey, AuthorizerService authorizer) =>
        {
            await authorizer.Authorize(apiKey);
            return handler.DynamicInvoke();
        });
    }

    public static void MapAuthorizedDelete(this WebApplication app, string route, Delegate handler)
    {
        app.UseApiKeys();
        app.MapDelete(route, async ([FromHeader(Name = "X-API-KEY")] string apiKey, AuthorizerService authorizer) =>
        {
            await authorizer.Authorize(apiKey);
            return handler.DynamicInvoke();
        });
    }

    private static void UseApiKeys(this WebApplication app)
    {
        if (_useApiKeysInitialized) return;
        _useApiKeysInitialized = true;

        _ = app.UseExceptionHandler(c => c.Run(async context =>
        {
            var exception = context?.Features?.Get<IExceptionHandlerPathFeature>()?.Error;
            var response = new { error = exception?.Message };
            await context.Response.WriteAsJsonAsync(response);
        }));

        _ = app.Use((context, next) =>
        {
            // Add X-API-KEY header to all requests
            if (!context.Request.Headers.ContainsKey("X-API-KEY"))
            {
                context.Request.Headers.Add("X-API-KEY", Guid.Empty.ToString());
            }
            return next();
        });
    }
}