using ConsoleCalc.Interfaces;

namespace ConsoleCalc.Services;

public class ConsoleService : IConsoleService
{
    public void WriteLine(string message) => Console.WriteLine(message);

    public void Write(string message) => Console.Write(message);

    public string? ReadLine() => Console.ReadLine();

    public void Clear() => Console.Clear();
}
