namespace WinFormsAspNetCoreSingleExe;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.SetCompatibleTextRenderingDefault(false);

        // Set default font
        Application.SetDefaultFont(new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point));

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }    
}