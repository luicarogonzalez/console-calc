using ConsoleCalc.Configuration;
using ConsoleCalc.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace ConsoleCalc.Tests;

public class CalculatorServiceTests
{
    private readonly IOptions<CalculatorSettings> _settings;
    private readonly CalculatorService _service;
    private readonly ITestOutputHelper _output;

    public CalculatorServiceTests(ITestOutputHelper output)
    {
        _output = output;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var settings = new CalculatorSettings();
        configuration.GetSection("CalculatorSettings").Bind(settings);
        
        _settings = Options.Create(settings);
        _service = new CalculatorService(_settings);
    }

    [Fact]
    public void Add_WithValidNumbers_ReturnsCorrectResult()
    {
        // Arrange
        var input = "3,5";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("3 + 5 = 8", result.Formula);
        Assert.Equal(8, result.Result);
    }

    [Fact]
    public void Add_WithEmptyValue_ConvertsToZero()
    {
        // Arrange
        var input = "3, ";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("3 + 0 = 3", result.Formula);
        Assert.Equal(3, result.Result);
    }

    [Fact]
    public void Add_WithInvalidValue_ConvertsToZero()
    {
        // Arrange
        var input = "3,abc";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("3 + 0 = 3", result.Formula);
        Assert.Equal(3, result.Result);
    }

    [Fact]
    public void Add_WithInvalidValueAtEnd_ConvertsToZero()
    {
        // Arrange
        var input = "4,dsfsdf";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("4 + 0 = 4", result.Formula);
        Assert.Equal(4, result.Result);
    }

    [Fact]
    public void Add_WithBothInvalidValues_ConvertsBothToZero()
    {
        // Arrange
        var input = "asdas,asdsa";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("0 + 0 = 0", result.Formula);
        Assert.Equal(0, result.Result);
    }

    [Fact]
    public void Add_WithInvalidValueAtStart_ConvertsToZero()
    {
        // Arrange
        var input = "asdsa,3";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("0 + 3 = 3", result.Formula);
        Assert.Equal(3, result.Result);
    }

    [Fact]
    public void Add_WithSingleNumber_ReturnsCorrectResult()
    {
        // Arrange
        var input = "42";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("42 = 42", result.Formula);
        Assert.Equal(42, result.Result);
    }

    [Fact]
    public void Add_WithNewlineSeparator_ReturnsCorrectResult()
    {
        // Arrange
        var input = "1\n2\n3";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("1 + 2 + 3 = 6", result.Formula);
        Assert.Equal(6, result.Result);
    }

    [Fact]
    public void Add_WithLiteralBackslashN_ConvertsToNewline()
    {
        // Arrange
        var input = "1\\n2\\n3";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("1 + 2 + 3 = 6", result.Formula);
        Assert.Equal(6, result.Result);
    }

    [Fact]
    public void Add_WithMultipleNewlines_TreatsEmptyAsZero()
    {
        // Arrange
        var input = "1\n\n3";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("1 + 0 + 3 = 4", result.Formula);
        Assert.Equal(4, result.Result);
    }

    [Fact]
    public void Add_WithMixedSeparators_ReturnsCorrectResult()
    {
        // Arrange
        var input = "1,2\n3";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("1 + 2 + 3 = 6", result.Formula);
        Assert.Equal(6, result.Result);
    }

    [Fact]
    public void Add_WithNegativeNumbersDisallowed_ThrowsException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CalculatorSettings:MaxNumbersAllowed"] = "0",
                ["CalculatorSettings:Separators:0"] = ",",
                ["CalculatorSettings:Separators:1"] = "\n",
                ["CalculatorSettings:AllowNegativeNumbers"] = "false"
            })
            .Build();

        var settings = new CalculatorSettings();
        config.GetSection("CalculatorSettings").Bind(settings);
        var service = new CalculatorService(Options.Create(settings));

        var input = "1,-2,3,-5,-7";

        // Act & Assert
        _output.WriteLine($"Input: {input}");
        var exception = Assert.Throws<InvalidOperationException>(() => service.Add(input));
        Assert.Equal("Negative numbers are not allowed: -2, -5, -7", exception.Message);
    }

    [Fact]
    public void Add_WithNumbersGreaterThanLimit_ConvertsToZero()
    {
        // Arrange - Using default settings with SkipNumbersGreaterThan = 1000
        var input = "2,1001,5";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("2 + 0 + 5 = 7", result.Formula);
        Assert.Equal(7, result.Result);
    }

    [Fact]
    public void Add_WithMultipleNumbersGreaterThanLimit_ConvertsAllToZero()
    {
        // Arrange
        var input = "2,1500,1001,3";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("2 + 0 + 0 + 3 = 5", result.Formula);
        Assert.Equal(5, result.Result);
    }

    [Fact]
    public void Add_WithNumberEqualToLimit_IsNotSkipped()
    {
        // Arrange - Number exactly equal to 1000 should NOT be skipped
        var input = "2,1000,5";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("2 + 1000 + 5 = 1007", result.Formula);
        Assert.Equal(1007, result.Result);
    }

    [Fact]
    public void Add_WithSkipDisabled_AllowsLargeNumbers()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CalculatorSettings:MaxNumbersAllowed"] = "0",
                ["CalculatorSettings:Separators:0"] = ",",
                ["CalculatorSettings:Separators:1"] = "\n",
                ["CalculatorSettings:AllowNegativeNumbers"] = "true",
                ["CalculatorSettings:SkipNumbersGreaterThan"] = "0"
            })
            .Build();

        var settings = new CalculatorSettings();
        config.GetSection("CalculatorSettings").Bind(settings);
        var service = new CalculatorService(Options.Create(settings));

        var input = "2,5000,3";

        // Act
        var result = service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("2 + 5000 + 3 = 5005", result.Formula);
        Assert.Equal(5005, result.Result);
    }
}
