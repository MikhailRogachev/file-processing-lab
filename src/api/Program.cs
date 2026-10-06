using api.Extensions;
using aws_agent.Extensions;
using data.Context;
using data.Services;
using domain.Interfaces.data;
using domain.Interfaces.Outbox;
using infrastructure.Oprions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration!;
var services = builder.Services;

// With SQL Server registration:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Get options
services.AddOptions<OutboxMessageOptions>().Bind(configuration.GetSection(nameof(OutboxMessageOptions)));

// Services
services.AddScoped<IMediaFileValidator, MediaFileValidator>();
services.AddScoped<IOutboxMessageBuilderService, OutboxMessageBuildService>();
services.AddScoped<IUnitOfWork, AppDbContext>();

// aws consumer
services.AwsServicesInjection(configuration);

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
