using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using RagChatPoC.Api.Data;
using RagChatPoC.Api.Repositories;
using RagChatPoC.Api.Services;
using RagChatPoC.Api.Services.Interfaces;
using Scalar.AspNetCore;

namespace RagChatPoC.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddDbContext<RagDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddHttpClient("OllamaClient", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["OLLAMA_HOST"] ?? string.Empty);
        });
        
        builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
        builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        builder.Services.AddScoped<IChatMessagesRepository, ChatMessageRepository>();
        builder.Services.AddScoped<IFileProcessingHelperService, FileProcessingHelperService>();
        builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
        builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
        builder.Services.AddScoped<IIndexService, IndexService>();
        builder.Services.AddScoped<IRagChatService, RagChatService>();
        builder.Services.AddScoped<IDocumentService, DocumentService>();
        
        // Add services to the container.
        builder.Services.AddAuthorization();

        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseAuthorization();

        app.MapControllers();
        
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RagDbContext>();
            dbContext.Database.Migrate(); // Applies pending migrations
        }

        app.Run();
    }
}