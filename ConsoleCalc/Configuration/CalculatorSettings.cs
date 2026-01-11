namespace ConsoleCalc.Configuration;

public class CalculatorSettings
{
    public int MaxNumbersAllowed { get; set; }
    public string[] Separators { get; set; } = Array.Empty<string>();
    public bool AllowNegativeNumbers { get; set; } = false;  
    public int SkipNumbersGreaterThan { get; set; }
}
