using System;
using System.Threading.Tasks;

namespace HelloWorld;

/// <summary>
/// Test commands for the HelloWorld sample app
/// </summary>
public class TestCommands
{
    public async Task<string> Echo(EchoArgs args)
    {
        await Task.Delay(100); // Simulate some work
        return $"Echo: {args.Message}";
    }
    
    public async Task<string> GetTime()
    {
        await Task.Delay(50);
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    public async Task<int> AddNumbers(AddNumbersArgs args)
    {
        await Task.Delay(50);
        return args.A + args.B;
    }
}

public class EchoArgs
{
    public string Message { get; set; } = "";
}

public class AddNumbersArgs
{
    public int A { get; set; }
    public int B { get; set; }
}