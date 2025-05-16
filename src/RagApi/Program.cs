using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Repositories;
using RagApi.Services;
using RagApi.Services.Interfaces;
using Scalar.AspNetCore;

namespace RagApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddDbContext<RagDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddHttpClient();
        
        builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
        builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        builder.Services.AddScoped<IChatMessagesRepository, ChatMessageRepository>();
        builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
        builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
        builder.Services.AddScoped<IIndexService, IndexService>();
        builder.Services.AddScoped<IChatService, ChatService>();
        
        // Add services to the container.
        builder.Services.AddAuthorization();

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

        app.Run();
    }
}