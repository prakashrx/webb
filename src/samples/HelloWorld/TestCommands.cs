using System;
using System.Threading.Tasks;

namespace HelloWorld;

public class GreetingArgs
{
    public string Name { get; set; } = "";
}

public class TestCommands
{
    public async Task<string> GetGreeting(GreetingArgs args)
    {
        await Task.Delay(100); // Simulate some work
        return $"Hello, {args.Name}! Welcome to WebUI Desktop.";
    }
}

