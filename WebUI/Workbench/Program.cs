using WebUI.Core.Windows;

namespace WebUI.Workbench;

static class Program
{
        /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        // Create and run the workbench entry point
        var workbenchEntry = new WorkbenchEntry();
        Application.Run(workbenchEntry);
    }    
}