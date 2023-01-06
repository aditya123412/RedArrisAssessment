
using Red_Arris_Assessment.Models;
using Red_Arris_Assessment.Services;

WebApplication app = Setup(args);

// NewMethodConfigure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static WebApplication Setup(string[] args)
{


    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var settings = builder.Configuration.GetSection("Settings").Get<Settings>();

    builder.Services.AddSingleton(new TickerPriceFetcherService(settings.BaseUrl, settings.PublicKey, settings));
    builder.Services.AddSingleton(settings);

    var app = builder.Build();
    return app;
}