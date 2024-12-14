using HCGStudio.PaddingStove.Core;
using HCGStudio.PaddingStove.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.TypeInfoResolverChain.Add(JsonContext.Default));
builder.Services.AddPaddingStoveCore();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services
    .AddCors(c => c.AddPolicy(
        "dev",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseCors("dev");
}

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();