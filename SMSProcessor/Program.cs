using Core.Interfaces;
using Core.Models;
using Gatekeeper.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using SMSGatekeeper;
using SMSProcessor.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var config = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json").Build();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register the Background Service
builder.Services.AddHostedService<SMSQueueConsumerService>();
builder.Services.AddSingleton<IDispatcher, SMSDidspatcher>();
builder.Services.AddSingleton<ISMSProcessorFactory, SMSProcessorFactory>();
builder.Services.AddSingleton<IStatisticRepository, StatisticReposritory>();
var appsettings = builder.Configuration.GetSection("DispatchConfiguration").Get<DispatchConfiguration>();
builder.Services.AddSingleton<IDispatchConfiguration>(appsettings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();