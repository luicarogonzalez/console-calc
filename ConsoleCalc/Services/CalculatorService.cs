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
        // Allow literal \n in input to be interpreted as newline separator
        input = input.Replace("\\n", "\n");
        
        var parts = input.Split(_settings.Separators, StringSplitOptions.None);
        var numbers = new List<int>();
        var negativeNumbers = new List<int>();

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || !int.TryParse(trimmed, out var number))
            {
                numbers.Add(0);
            }
            else
            {
                // Collect negative numbers if not allowed
                if (!_settings.AllowNegativeNumbers && number < 0)
                {
                    negativeNumbers.Add(number);
                }
                
                numbers.Add(number);
            }
        }

        // Validate negative numbers if not allowed
        if (!_settings.AllowNegativeNumbers && negativeNumbers.Count > 0)
        {
            var negativeList = string.Join(", ", negativeNumbers);
            throw new InvalidOperationException($"Negative numbers are not allowed: {negativeList}");
        }

        // Validate number count (0 means no limit)
        if (_settings.MaxNumbersAllowed > 0 && numbers.Count > _settings.MaxNumbersAllowed)
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
