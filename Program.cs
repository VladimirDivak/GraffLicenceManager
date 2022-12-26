using GraffLicenceManager;
using GraffLicenceManager.DB;
using GraffLicenceManager.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<MailSender>();
builder.Services.AddScoped(x =>
{
    var navManager = x.GetRequiredService<NavigationManager>();
    return new HubConnectionBuilder()
        .WithUrl(navManager.ToAbsoluteUri("/adminhub"))
        .Build();
});

var app = builder.Build();

app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
    endpoints.MapHub<UserHub>("/userhub");
    endpoints.MapHub<AdminHub>("/adminhub");
});

app.Run();


public enum CardPosition
{
    Top,
    Center,
    Bottom,
    Full
}