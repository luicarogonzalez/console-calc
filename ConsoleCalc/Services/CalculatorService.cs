using ConsoleCalc.Configuration;
using ConsoleCalc.Interfaces;
using ConsoleCalc.Models;
using Microsoft.Extensions.Options;

namespace ConsoleCalc.Services;

public class CalculatorService : ICalculatorService
{
    private readonly CalculatorSettings _settings;

    public CalculatorService(IOptions<CalculatorSettings> settings)
    {
        _settings = settings.Value;
    }

    public CalculatorResult Add(string input)
    {
        var parts = input.Split(_settings.Separator);
        var numbers = new List<int>();

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || !int.TryParse(trimmed, out var number))
            {
                numbers.Add(0);
            }
            else
            {
                numbers.Add(number);
            }
        }

        // Validate number count
        if (numbers.Count > _settings.MaxNumbersAllowed)
            throw new InvalidOperationException($"No more than {_settings.MaxNumbersAllowed} numbers are allowed");

        var result = numbers.Sum();
        var formula = string.Join(" + ", numbers) + " = " + result;

        return new CalculatorResult
        {
            Formula = formula,
            Result = result
        };
    }
}
