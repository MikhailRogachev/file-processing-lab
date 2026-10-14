using api.Extensions;
using aws_agent.Extensions;
using data.Extensions;
using data.Services;
using domain.Interfaces.Outbox;
using infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration!;
var services = builder.Services;

// Services
services.AddScoped<IMediaFileValidator, MediaFileValidator>();
services.AddScoped<IOutboxMessageBuilderService, OutboxMessageBuildService>();

// aws consumer
services.AwsServicesInjection(configuration);
services.DataServicesInjection(configuration);
services.InfrastructureServicesRegistration(configuration);

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MigrateDatabase(); // Migrate the database on application startup

app.MapControllers();

app.Run();
