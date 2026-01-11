namespace ConsoleCalc.Configuration;

public class CalculatorSettings
{
    public int MaxNumbersAllowed { get; set; }
    public string[] Separators { get; set; } = Array.Empty<string>();
}
