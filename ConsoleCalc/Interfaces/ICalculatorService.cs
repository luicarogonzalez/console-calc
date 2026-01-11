using ConsoleCalc.Models;

namespace ConsoleCalc.Interfaces;

public interface ICalculatorService
{
    CalculatorResult Add(string input);
}
