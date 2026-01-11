namespace ConsoleCalc.Interfaces;

public interface IConsoleService
{
    void WriteLine(string message);
    void Write(string message);
    string? ReadLine();
    void Clear();
}
