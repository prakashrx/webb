using WebUI.Framework;

namespace WebUI.Framework.Examples;

/// <summary>
/// Example demonstrating the simple BrowserWindow API
/// </summary>
public static class BrowserWindowExample
{
    /// <summary>
    /// Create a simple browser window and navigate to a URL
    /// </summary>
    public static async Task CreateSimpleWindow()
    {
        // Create a browser window - similar to Tauri's API
        using var window = new BrowserWindow("My App", 1200, 800);
        
        // Navigate to a URL
        await window.NavigateAsync("https://example.com");
        
        // Show the window
        window.Show();
    }

    /// <summary>
    /// Create a browser window with HTML content
    /// </summary>
    public static async Task CreateHtmlWindow()
    {
        using var window = new BrowserWindow("HTML Demo", 800, 600);
        
        // Load HTML content
        await window.LoadHtmlAsync("""
            <html>
            <head>
                <title>Demo</title>
                <style>
                    body { font-family: Arial, sans-serif; padding: 20px; }
                    .container { max-width: 600px; margin: 0 auto; }
                    button { padding: 10px 20px; margin: 10px; }
                </style>
            </head>
            <body>
                <div class="container">
                    <h1>Browser Window Demo</h1>
                    <p>This is a simple browser window created with BrowserWindow.</p>
                    <button onclick="sendMessage()">Send Message to Host</button>
                    <button onclick="changeTitle()">Change Title</button>
                </div>
                <script>
                    function sendMessage() {
                        window.chrome.webview.postMessage('Hello from JavaScript!');
                    }
                    
                    function changeTitle() {
                        document.title = 'Title Changed!';
                    }
                </script>
            </body>
            </html>
            """);
        
        // Handle messages from JavaScript
        window.MessageReceived += (sender, message) =>
        {
            Console.WriteLine($"Received message: {message}");
        };
        
        window.Show();
    }

    /// <summary>
    /// Create a browser window with host object interop
    /// </summary>
    public static async Task CreateWindowWithHostObject()
    {
        using var window = new BrowserWindow("Host Object Demo", 800, 600);
        
        // Add host object for JavaScript interop
        var hostApi = new HostApi();
        await window.AddHostObjectAsync("hostApi", hostApi);
        
        await window.LoadHtmlAsync("""
            <html>
            <head>
                <title>Host Object Demo</title>
                <style>
                    body { font-family: Arial, sans-serif; padding: 20px; }
                    .container { max-width: 600px; margin: 0 auto; }
                    button { padding: 10px 20px; margin: 10px; }
                </style>
            </head>
            <body>
                <div class="container">
                    <h1>Host Object Demo</h1>
                    <p>Call C# methods from JavaScript:</p>
                    <button onclick="callHostMethod()">Call Host Method</button>
                    <div id="result"></div>
                </div>
                <script>
                    async function callHostMethod() {
                        try {
                            const result = await window.chrome.webview.hostObjects.hostApi.GetMessage();
                            document.getElementById('result').innerHTML = '<p>Result: ' + result + '</p>';
                        } catch (error) {
                            document.getElementById('result').innerHTML = '<p>Error: ' + error + '</p>';
                        }
                    }
                </script>
            </body>
            </html>
            """);
        
        window.Show();
    }

    /// <summary>
    /// Create multiple browser windows
    /// </summary>
    public static async Task CreateMultipleWindows()
    {
        var windows = new List<BrowserWindow>();
        
        for (int i = 0; i < 3; i++)
        {
            var window = new BrowserWindow($"Window {i + 1}", 400, 300);
            window.SetPosition(100 + i * 50, 100 + i * 50);
            
            await window.LoadHtmlAsync($"""
                <html>
                <body style="font-family: Arial, sans-serif; padding: 20px;">
                    <h2>Window {i + 1}</h2>
                    <p>This is window number {i + 1}</p>
                    <button onclick="alert('Hello from window {i + 1}!')">Click Me</button>
                </body>
                </html>
                """);
            
            window.Show();
            windows.Add(window);
        }
        
        // Clean up when done
        // In a real app, you'd handle this differently
        Console.WriteLine("Press Enter to close all windows...");
        Console.ReadLine();
        
        foreach (var window in windows)
        {
            window.Dispose();
        }
    }
}

/// <summary>
/// Example host API class for JavaScript interop
/// </summary>
public class HostApi
{
    public string GetMessage()
    {
        return $"Hello from C# at {DateTime.Now:HH:mm:ss}";
    }
    
    public void ShowNotification(string message)
    {
        Console.WriteLine($"Notification: {message}");
    }
} 