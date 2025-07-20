using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
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
    
    /// <summary>
    /// Auto-register all command classes from an assembly
    /// </summary>
    public static void AutoRegister(Assembly assembly)
    {
        // Find all types ending with "Commands"
        var commandTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Commands"))
            .ToList();
            
        Console.WriteLine($"Found {commandTypes.Count} command classes");
        
        foreach (var type in commandTypes)
        {
            RegisterCommandClass(type);
        }
    }
    
    /// <summary>
    /// Register all methods from a command class
    /// </summary>
    private static void RegisterCommandClass(Type type)
    {
        // Create instance (support both parameterless and DI constructors)
        var instance = Activator.CreateInstance(type);
        if (instance == null) return;
        
        // Get the prefix from class name (e.g., "WindowCommands" -> "window")
        var prefix = type.Name.Replace("Commands", "");
        if (prefix == type.Name) prefix = type.Name; // If it doesn't end with Commands
        prefix = char.ToLower(prefix[0]) + prefix.Substring(1); // camelCase
        
        // Register all public methods
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && m.DeclaringType == type); // Skip property getters/setters
            
        foreach (var method in methods)
        {
            var commandName = $"{prefix}.{char.ToLower(method.Name[0]) + method.Name.Substring(1)}";
            Console.WriteLine($"Registering command: {commandName}");
            RegisterMethod(commandName, instance, method);
        }
    }
    
    /// <summary>
    /// Register a single method as a command
    /// </summary>
    private static void RegisterMethod(string commandName, object instance, MethodInfo method)
    {
        _commands[commandName] = async (argsJson) =>
        {
            var parameters = method.GetParameters();
            object?[] args;
            
            if (parameters.Length == 0)
            {
                // No parameters
                args = Array.Empty<object>();
            }
            else if (parameters.Length == 1)
            {
                // Single parameter - deserialize the JSON to that type
                if (argsJson.HasValue && argsJson.Value.ValueKind != JsonValueKind.Null)
                {
                    var json = argsJson.Value.GetRawText();
                    var paramType = parameters[0].ParameterType;
                    var arg = JsonSerializer.Deserialize(json, paramType, JsonOptions);
                    args = new[] { arg };
                }
                else
                {
                    args = new object?[] { null };
                }
            }
            else
            {
                // Multiple parameters - expect a JSON object with named properties
                throw new NotSupportedException($"Command '{commandName}' has multiple parameters. Use a single parameter object instead.");
            }
            
            // Invoke the method
            var result = method.Invoke(instance, args);
            
            // Handle async methods
            if (result is Task task)
            {
                await task;
                
                // Get the result from Task<T>
                var taskType = task.GetType();
                if (taskType.IsGenericType)
                {
                    var resultProperty = taskType.GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
                return null;
            }
            
            return result;
        };
    }
}