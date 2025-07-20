using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.WinForms;
using WebUI.Commands;

namespace WebUI.Bridge;

/// <summary>
/// Core API for command invocation
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class CoreApi
{
    private readonly WebView2 _webView;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    
    public CoreApi(WebView2 webView)
    {
        _webView = webView;
    }
    
    /// <summary>
    /// Invoke a command from JavaScript
    /// </summary>
    public void InvokeCommand(string command, string requestJson)
    {
        Console.WriteLine($"InvokeCommand called: {command}");
        Console.WriteLine($"Request JSON: {requestJson}");
        
        // Parse the request
        InvokeRequest request;
        try
        {
            request = JsonSerializer.Deserialize<InvokeRequest>(requestJson, JsonOptions) 
                ?? throw new ArgumentException("Invalid request");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse invoke request: {ex.Message}");
            return;
        }
        
        Console.WriteLine($"Parsed request - Id: {request.Id}, Command: {request.Command}");
        
        // Execute command asynchronously
        Task.Run(async () =>
        {
            Console.WriteLine($"Executing command: {command}");
            InvokeResponse response;
            try
            {
                var result = await CommandRegistry.ExecuteAsync(command, request.Args);
                Console.WriteLine($"Command result: {result}");
                response = new InvokeResponse
                {
                    Id = request.Id,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Command error: {ex.Message}");
                response = new InvokeResponse
                {
                    Id = request.Id,
                    Error = ex.Message
                };
            }
            
            // Send response back to JavaScript
            var responseJson = JsonSerializer.Serialize(response, JsonOptions);
            _webView.BeginInvoke(() =>
            {
                _webView.CoreWebView2.PostWebMessageAsJson(responseJson);
            });
        });
    }
}

internal class InvokeRequest
{
    public string Id { get; set; } = "";
    public string Command { get; set; } = "";
    public JsonElement? Args { get; set; }
}

internal class InvokeResponse
{
    public string Id { get; set; } = "";
    public object? Result { get; set; }
    public string? Error { get; set; }
}