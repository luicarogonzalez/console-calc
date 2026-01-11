using ConsoleCalc.Models;

namespace ConsoleCalc.Interfaces;

public interface ICalculatorService
{
    CalculatorResult Add(string input);
    CalculatorResult Subtract(string input);
    CalculatorResult Multiply(string input);
    CalculatorResult Divide(string input);
}
