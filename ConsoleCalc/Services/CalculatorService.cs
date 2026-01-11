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
        
        // Check for custom delimiter
        var separators = _settings.Separators;
        if (!string.IsNullOrEmpty(_settings.CustomDelimiter.Prefix) && input.StartsWith(_settings.CustomDelimiter.Prefix))
        {
            var prefixLength = _settings.CustomDelimiter.Prefix.Length;
            var newlineIndex = input.IndexOf('\n');
            
            if (newlineIndex > prefixLength)
            {
                var customDelimiter = input.Substring(prefixLength, newlineIndex - prefixLength);
                
                // Validate custom delimiter length
                if (customDelimiter.Length > _settings.CustomDelimiter.MaxLength)
                {
                    throw new InvalidOperationException($"Custom delimiter exceeds maximum length of {_settings.CustomDelimiter.MaxLength}");
                }
                
                // Add custom delimiter to existing separators
                separators = _settings.Separators.Concat(new[] { customDelimiter }).ToArray();
                input = input.Substring(newlineIndex + 1);
            }
        }
        
        var parts = input.Split(separators, StringSplitOptions.None);
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
                
                // Skip numbers greater than the configured limit (0 means no limit)
                if (_settings.SkipNumbersGreaterThan > 0 && number > _settings.SkipNumbersGreaterThan)
                {
                    numbers.Add(0);
                }
                else
                {
                    numbers.Add(number);
                }
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
