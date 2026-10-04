using Amazon.SQS;
using api.Extensions;
using aws_agent.Options;
using aws_agent.Services;
using data.Config;
using data.Context;
using data.Services;
using domain.Interfaces.data;
using domain.Interfaces.Messaging;
using domain.Interfaces.Outbox;
using floci_management.BackgroundServices;
using infrastructure.Oprions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration!;
var services = builder.Services;

// Add services to the container.
var connString = configuration.GetSection(nameof(DatabaseConfig)).Get<DatabaseConfig>()!.BuildNpgsqlConnectionString();

// Data context
services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connString));

// Get options
services.AddOptions<AwsOptions>().Bind(configuration.GetSection(nameof(AwsOptions)));
services.AddOptions<OutboxMessageOptions>().Bind(configuration.GetSection(nameof(OutboxMessageOptions)));

// Services
services.AddScoped<IMessageConsumer<AmazonSQSClient>, SqsMessageConsumer>();
services.AddScoped<IMediaFileValidator, MediaFileValidator>();
services.AddScoped<IOutboxMessageBuilderService, OutboxMessageBuildService>();
services.AddScoped<IUnitOfWork, AppDbContext>();

// Background services
services.AddHostedService<SqsListener>();

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
