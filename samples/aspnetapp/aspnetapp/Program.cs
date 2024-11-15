using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();

// Uncomment if using System.Text.Json source generation
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
// });

var app = builder.Build();

app.MapHealthChecks("/healthz");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

CancellationTokenSource cancellation = new();
app.Lifetime.ApplicationStopping.Register( () =>
{
    cancellation.Cancel();
});

app.MapGet("/test", (context) =>
{
    var remoteIp = context.Connection.RemoteIpAddress;
    var iPAddress = GetRemoteHostIpAddressUsingXForwardedFor(context);
    var headers = context.Request.Headers;
    var response = "<html><body><h1>Request Headers</h1><ul>";
    foreach (var header in headers)
    {
        response += $"<li><strong>{header.Key}:</strong> {header.Value}</li>";
    }
    response += "</ul>";
    response += "<h1>Remote IP</h1><ul>";
    response += $"<li>{remoteIp}</li>";
    response += "</ul>";
    response += "<h1>x-Forwarder-For</h1><ul>";
    response += $"<li>{iPAddress}</li>";
    response += "</ul></body></html>";
    return context.Response.WriteAsync(response);
});

app.MapGet("/Environment", () =>
{
    return new EnvironmentInfo();
});

// This API demonstrates how to use task cancellation
// to support graceful container shutdown via SIGTERM.
// The method itself is an example and not useful.
app.MapGet("/Delay/{value}", async (int value) =>
{
    try
    {
        await Task.Delay(value, cancellation.Token);
    }
    catch(TaskCanceledException)
    {
    }

    return new Operation(value);
});

app.Run();

static IPAddress? GetRemoteHostIpAddressUsingXForwardedFor(HttpContext httpContext)
{
    IPAddress? remoteIpAddress = null;
    var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (!string.IsNullOrEmpty(forwardedFor))
    {
        var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => s.Trim());
        foreach (var ip in ips)
        {
            if (IPAddress.TryParse(ip, out var address) &&
                (address.AddressFamily is AddressFamily.InterNetwork
                 or AddressFamily.InterNetworkV6))
            {
                remoteIpAddress = address;
                break;
            }
        }
    }
    return remoteIpAddress;
}

[JsonSerializable(typeof(EnvironmentInfo))]
[JsonSerializable(typeof(Operation))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}

public record struct Operation(int Delay);
