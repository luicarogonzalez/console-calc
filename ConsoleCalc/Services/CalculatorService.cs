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
        input = NormalizeInput(input);
        var (processedInput, separators) = ExtractCustomDelimiter(input);
        var (numbers, negativeNumbers) = ParseNumbers(processedInput, separators);
        ValidateNumbers(numbers, negativeNumbers);
        
        return CalculateResult(numbers, (nums) => nums.Sum(), "+");
    }

    public CalculatorResult Subtract(string input)
    {
        input = NormalizeInput(input);
        var (processedInput, separators) = ExtractCustomDelimiter(input);
        var (numbers, negativeNumbers) = ParseNumbers(processedInput, separators);
        ValidateNumbers(numbers, negativeNumbers);
        
        return CalculateResult(numbers, (nums) => nums.First() - nums.Skip(1).Sum(), "-");
    }

    public CalculatorResult Multiply(string input)
    {
        input = NormalizeInput(input);
        var (processedInput, separators) = ExtractCustomDelimiter(input);
        var (numbers, negativeNumbers) = ParseNumbers(processedInput, separators);
        ValidateNumbers(numbers, negativeNumbers);
        
        return CalculateResult(numbers, (nums) => nums.Aggregate(1, (acc, n) => acc * n), "*");
    }

    public CalculatorResult Divide(string input)
    {
        input = NormalizeInput(input);
        var (processedInput, separators) = ExtractCustomDelimiter(input);
        var (numbers, negativeNumbers) = ParseNumbers(processedInput, separators);
        ValidateNumbers(numbers, negativeNumbers);
        
        if (numbers.Skip(1).Any(n => n == 0))
        {
            throw new InvalidOperationException("Cannot divide by zero");
        }
        
        return CalculateResult(numbers, (nums) => nums.Skip(1).Aggregate(nums.First(), (acc, n) => acc / n), "/");
    }

    private string NormalizeInput(string input)
    {
        // Allow literal \n in input to be interpreted as newline separator
        return input.Replace("\\n", "\n");
    }

    private (string input, string[] separators) ExtractCustomDelimiter(string input)
    {
        var separators = _settings.Separators;
        
        if (string.IsNullOrEmpty(_settings.CustomDelimiter.Prefix) || !input.StartsWith(_settings.CustomDelimiter.Prefix))
        {
            return (input, separators);
        }

        var prefixLength = _settings.CustomDelimiter.Prefix.Length;
        var newlineIndex = input.IndexOf('\n');
        
        if (newlineIndex <= prefixLength)
        {
            return (input, separators);
        }

        var delimiterSection = input.Substring(prefixLength, newlineIndex - prefixLength);
        var customDelimiters = ExtractDelimitersFromSection(delimiterSection);
        
        separators = _settings.Separators.Concat(customDelimiters).ToArray();
        input = input.Substring(newlineIndex + 1);
        
        return (input, separators);
    }

    private List<string> ExtractDelimitersFromSection(string delimiterSection)
    {
        var delimiters = new List<string>();
        
        // Check if we have multiple bracketed delimiters
        if (_settings.CustomDelimiter.SupportBrackets && delimiterSection.Contains('['))
        {
            var i = 0;
            while (i < delimiterSection.Length)
            {
                if (delimiterSection[i] == '[')
                {
                    var closingIndex = delimiterSection.IndexOf(']', i);
                    if (closingIndex == -1)
                    {
                        throw new InvalidOperationException("Unclosed bracket in custom delimiter");
                    }
                    
                    var delimiter = delimiterSection.Substring(i + 1, closingIndex - i - 1);
                    delimiters.Add(delimiter);
                    i = closingIndex + 1;
                }
                else
                {
                    i++;
                }
            }
            
            if (delimiters.Count > 0)
            {
                return delimiters;
            }
        }
        
        // Single non-bracketed delimiter
        if (_settings.CustomDelimiter.MaxLength > 0 && delimiterSection.Length > _settings.CustomDelimiter.MaxLength)
        {
            throw new InvalidOperationException($"Custom delimiter exceeds maximum length of {_settings.CustomDelimiter.MaxLength}");
        }
        
        delimiters.Add(delimiterSection);
        return delimiters;
    }

    private (List<int> numbers, List<int> negativeNumbers) ParseNumbers(string input, string[] separators)
    {
        var parts = input.Split(separators, StringSplitOptions.None);
        var numbers = new List<int>();
        var negativeNumbers = new List<int>();

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            
            if (string.IsNullOrWhiteSpace(trimmed) || !int.TryParse(trimmed, out var number))
            {
                numbers.Add(0);
                continue;
            }

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

        return (numbers, negativeNumbers);
    }

    private void ValidateNumbers(List<int> numbers, List<int> negativeNumbers)
    {
        // Validate negative numbers if not allowed
        if (!_settings.AllowNegativeNumbers && negativeNumbers.Count > 0)
        {
            var negativeList = string.Join(", ", negativeNumbers);
            throw new InvalidOperationException($"Negative numbers are not allowed: {negativeList}");
        }

        // Validate number count (0 means no limit)
        if (_settings.MaxNumbersAllowed > 0 && numbers.Count > _settings.MaxNumbersAllowed)
        {
            throw new InvalidOperationException($"No more than {_settings.MaxNumbersAllowed} numbers are allowed");
        }
    }

    private CalculatorResult CalculateResult(List<int> numbers, Func<List<int>, int> operation, string operatorSymbol)
    {
        var result = operation(numbers);
        var formula = string.Join($" {operatorSymbol} ", numbers) + " = " + result;

        return new CalculatorResult
        {
            Formula = formula,
            Result = result
        };
    }
}
