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
    public void Add_WithNegativeNumbers_ReturnsCorrectResult()
    {
        // Arrange
        var input = "-5,3";

        // Act
        var result = _service.Add(input);

        // Assert
        _output.WriteLine($"Input: {input}");
        _output.WriteLine($"Formula: {result.Formula}");
        Assert.Equal("-5 + 3 = -2", result.Formula);
        Assert.Equal(-2, result.Result);
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
}
