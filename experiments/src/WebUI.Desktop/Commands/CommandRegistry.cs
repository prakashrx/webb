using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebUI.Commands;

/// <summary>
/// Registry for all available commands
/// </summary>
public static class CommandRegistry
{
    private static readonly ConcurrentDictionary<string, Func<JsonElement?, Task<object?>>> _commands = new();
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Register a command with arguments
    /// </summary>
    public static void Register<TArgs, TResult>(string name, Func<TArgs, Task<TResult>> handler)
        where TArgs : class
    {
        _commands[name] = async (argsJson) =>
        {
            TArgs args;
            if (argsJson.HasValue && argsJson.Value.ValueKind != JsonValueKind.Null)
            {
                var json = argsJson.Value.GetRawText();
                args = JsonSerializer.Deserialize<TArgs>(json, JsonOptions) ?? throw new ArgumentException($"Invalid arguments for command '{name}'");
            }
            else
            {
                throw new ArgumentException($"Command '{name}' requires arguments");
            }
            
            var result = await handler(args);
            return result;
        };
    }

    /// <summary>
    /// Register a command with no arguments
    /// </summary>
    public static void Register<TResult>(string name, Func<Task<TResult>> handler)
    {
        _commands[name] = async (_) =>
        {
            var result = await handler();
            return result;
        };
    }

    /// <summary>
    /// Execute a command
    /// </summary>
    public static async Task<object?> ExecuteAsync(string command, JsonElement? args)
    {
        if (_commands.TryGetValue(command, out var handler))
        {
            return await handler(args);
        }
        
        throw new InvalidOperationException($"Command '{command}' not found");
    }

    /// <summary>
    /// Check if a command exists
    /// </summary>
    public static bool Exists(string command) => _commands.ContainsKey(command);
}