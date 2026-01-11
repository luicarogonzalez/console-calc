using ConsoleCalc;
using ConsoleCalc.Configuration;
using ConsoleCalc.Interfaces;
using ConsoleCalc.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure configuration file reading
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Configure dependency injection container
var services = new ServiceCollection();

// Register configurations
services.Configure<CalculatorSettings>(configuration.GetSection("CalculatorSettings"));

// Register services
services.AddSingleton<ICalculatorService, CalculatorService>();
services.AddSingleton<IConsoleService, ConsoleService>();
services.AddSingleton<Application>();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Resolve and run the application
var app = serviceProvider.GetRequiredService<Application>();
app.Run();
