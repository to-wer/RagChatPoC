using RagChatPoC.Web.Components;
using RagChatPoC.Web.ViewModels;

namespace RagChatPoC.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
        
        builder.Services.AddHttpClient("RagChatPoC.Api", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["RAGCHAT_API_BASEURL"] ?? string.Empty);
            client.Timeout = TimeSpan.FromMinutes(5); // adjust as needed
        });

        builder.Services.AddScoped<ChatViewModel>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}