using ConsoleCalc.Configuration;
using ConsoleCalc.Interfaces;
using Microsoft.Extensions.Options;

namespace ConsoleCalc;

public class Application
{
    private readonly ICalculatorService _calculatorService;
    private readonly IConsoleService _consoleService;
    private readonly CalculatorSettings _settings;

    public Application(
        ICalculatorService calculatorService, 
        IConsoleService consoleService,
        IOptions<CalculatorSettings> settings)
    {
        _calculatorService = calculatorService;
        _consoleService = consoleService;
        _settings = settings.Value;
    }

    public void Run()
    {
        _consoleService.WriteLine("Console Calculator");
        _consoleService.WriteLine("===========================");
        _consoleService.WriteLine("");

        while (true)
        {
            //showing active separators from settings
            var displaySeparators = _settings.Separators.Select(s => s.Replace("\n", "\\n")).ToArray();
            _consoleService.Write($"Enter numbers separated by: {string.Join(" or ", displaySeparators)}: ");
            var input = _consoleService.ReadLine();

            try
            {
                var result = _calculatorService.Add(input);
                _consoleService.WriteLine(result.Formula);
            }
            catch (Exception ex)
            {
                _consoleService.WriteLine($"Error: {ex.Message}");
            }

            _consoleService.WriteLine(""); // enter a blank line for spacing
        }
    }
}
