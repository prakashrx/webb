using System.Threading.Tasks;

namespace WebUI.Commands;

/// <summary>
/// Base interface for all WebUI commands
/// </summary>
public interface ICommand
{
    string Name { get; }
}

/// <summary>
/// Command with input arguments and return value
/// </summary>
public interface ICommand<TArgs, TResult> : ICommand
{
    Task<TResult> ExecuteAsync(TArgs args);
}

/// <summary>
/// Command with no arguments but has return value
/// </summary>
public interface ICommand<TResult> : ICommand
{
    Task<TResult> ExecuteAsync();
}