using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

namespace WinFormsAspNetCoreSingleExe;

/// <summary>
/// Main form for the application that hosts an embedded ASP.NET Core web server.
/// The web server lifecycle is managed alongside the form's lifetime.
/// </summary>
public partial class MainForm : Form
{
    private readonly IntPtr _consoleWindowHandlePtr;
    private bool _consoleWindowIsHidden;
    private bool _consoleCloseButtonIsDisabled;

    private bool _aspNetCoreAppHostIsRunning = false;

    private IHost? _webHost;
    private Task? _webHostTask = null;

    private volatile bool _isFormClosing = false;

    /// <summary>
    /// Initializes a new instance of the MainForm class.
    /// </summary>
    /// <param name="startWithConsoleHidden">If true, hides the console window on startup.</param>
    public MainForm(bool startWithConsoleHidden = true)
    {
        _consoleWindowHandlePtr = Win32Interop.GetConsoleWindow();
        _consoleWindowIsHidden = !Win32Interop.IsWindowVisible(_consoleWindowHandlePtr);
        DisableConsoleCloseButton();
        SetConsolePositionRelativeToForm();

        if (startWithConsoleHidden && !_consoleWindowIsHidden)
        {
            HideConsoleWindow();
        }

        InitializeComponent();
        SetConsoleWindowTogglerButtonLabel();
    }

    #region AspNetCore Host
    /// <summary>
    /// Initializes the ASP.NET Core web host with configured services and lifetime events.
    /// </summary>
    private void InitializeAspNetCoreHost()
    {
        _webHost = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<AspNetCoreStartup>();
            })
            .Build();

        // Register application lifetime events
        var lifetime = _webHost.Services.GetService(typeof(IHostApplicationLifetime)) as IHostApplicationLifetime;

        lifetime!.ApplicationStarted.Register(() =>
        {
            Invoke(new Action(() =>
            {
                aspNetCoreHostTogglerButton.Enabled = true;
                var url = GetAspNetCoreHostUrl();
                if (!string.IsNullOrEmpty(url))
                {
                    var webHostUrlLinkLabelTextPrefix = "AspNetCore Host URL";
                    webHostUrlLinkLabel.Text = $"{webHostUrlLinkLabelTextPrefix}: {url}";
                    webHostUrlLinkLabel.Links.Clear();
                    webHostUrlLinkLabel.Links.Add(webHostUrlLinkLabelTextPrefix.Length + 2, url.Length, url); // Make the URL part clickable
                }
                _aspNetCoreAppHostIsRunning = true;
            }));
        });

        lifetime!.ApplicationStopped.Register(() =>
        {
            if (!_isFormClosing)
            {
                Invoke(new Action(() =>
                {
                    webHostUrlLinkLabel.Text = "AspNetCore Host is not running";
                    webHostUrlLinkLabel.Links.Clear();
                    _aspNetCoreAppHostIsRunning = false;
                    aspNetCoreHostTogglerButton.Enabled = true;
                }));
            }
        });
    }

    /// <summary>
    /// Starts the ASP.NET Core web host on a background thread and waits for it to be fully started.
    /// </summary>
    private async Task StartAspNetCoreHost()
    {
        InitializeAspNetCoreHost();

        // Create a task completion source to signal when started
        var startedTcs = new TaskCompletionSource<bool>();

        var lifecycle = _webHost!.Services.GetService(typeof(IHostApplicationLifetime)) as IHostApplicationLifetime;
        lifecycle!.ApplicationStarted.Register(() => startedTcs.TrySetResult(true));

        // Start the host on background thread
        _webHostTask = Task.Run(async () => await _webHost!.RunAsync());

        // Wait for the startup to complete
        await startedTcs.Task;
    }

    /// <summary>
    /// Gracefully stops the ASP.NET Core web host and cleans up resources.
    /// </summary>
    /// <returns>A task that completes when the host has fully stopped.</returns>
    private async Task StopAspNetCoreHost()
    {
        if (_webHost == null) return;

        var stopTask = _webHost!.StopAsync(); // capture but don't await yet

        if (_webHostTask != null)
        {
            await _webHostTask;
        }
        else
        {
            await stopTask; // Ensure stop completed successfully
        }

        _webHost?.Dispose();
        _webHost = null;
    }

    /// <summary>
    /// Gets the URL where the ASP.NET Core web host is listening.
    /// </summary>
    /// <returns>The server URL with localhost substituted for wildcard addresses, or null</returns>
    private string? GetAspNetCoreHostUrl()
    {
        var serverAddresses = _webHost!.Services.GetService(typeof(IServer)) as IServer;
        var addressesFeature = serverAddresses?.Features.Get<IServerAddressesFeature>();
        var url = addressesFeature?.Addresses?.FirstOrDefault()?
            .Replace("[::]", "localhost")
            .Replace("0.0.0.0", "localhost");

        return url;
    }
    #endregion AspNetCore Host

    #region UI Helpers
    /// <summary>
    /// Updates the console window toggler button text based on current visibility state.
    /// </summary>
    private void SetConsoleWindowTogglerButtonLabel()
    {
        if (_consoleWindowIsHidden)
        {
            consoleWindowTogglerButton.Text = "Show Console Window";
        }
        else
        {
            consoleWindowTogglerButton.Text = "Hide Console Window";
        }
    }

    /// <summary>
    /// Toggles the ASP.NET Core web host on or off asynchronously without blocking the UI.
    /// </summary>
    private async Task AspNetCoreHostToggler()
    {
        aspNetCoreHostTogglerButton.Enabled = false;

        if (_aspNetCoreAppHostIsRunning)
        {
            await StopAspNetCoreHost();
            aspNetCoreHostTogglerButton.Text = "Start AspNetCore Host";
        }
        else
        {
            await StartAspNetCoreHost();
            aspNetCoreHostTogglerButton.Text = "Stop AspNetCore Host";
        }

        // IHostApplicationLifetime Events will handle re-enabling aspNetCoreHostToggleButton 
    }
    #endregion UI Helpers

    #region Console Management Functions
    /// <summary>
    /// Disables the close button on the console window to prevent accidental termination.
    /// </summary>
    private void DisableConsoleCloseButton()
    {
        Win32Interop.DisableWindowCloseButton(_consoleWindowHandlePtr);
        _consoleCloseButtonIsDisabled = true;
    }

    /// <summary>
    /// Re-enables the close button on the console window.
    /// </summary>
    private void EnableConsoleCloseButton()
    {
        Win32Interop.EnableWindowCloseButton(_consoleWindowHandlePtr);
        _consoleCloseButtonIsDisabled = false;
    }

    /// <summary>
    /// Makes the console window visible and positions it relative to the form.
    /// </summary>
    private void ShowConsoleWindow()
    {
        Win32Interop.RestoreWindowIfMinimized(_consoleWindowHandlePtr);
        SetConsolePositionRelativeToForm();
        Win32Interop.ShowWindow(_consoleWindowHandlePtr);
        _consoleWindowIsHidden = false;
    }

    /// <summary>
    /// Hide the console window from view.
    /// </summary>
    private void HideConsoleWindow()
    {
        Win32Interop.HideWindow(_consoleWindowHandlePtr);
        _consoleWindowIsHidden = true;
    }

    /// <summary>
    /// Restores the console window to its default state before application exit.
    /// Shows the window if hidden and enables the close button if disabled.
    /// </summary>
    private void ResetConsoleWindowState()
    {
        if (_consoleWindowIsHidden)
        {
            ShowConsoleWindow();
            _consoleWindowIsHidden = false;
        }

        if (_consoleCloseButtonIsDisabled)
        {
            EnableConsoleCloseButton();
            _consoleCloseButtonIsDisabled = false;
        }
    }

    /// <summary>
    /// Positions the console window adjacent to the form.
    /// </summary>
    private void SetConsolePositionRelativeToForm()
    {
        var formLocation = this.Location;
        var formSize = this.Size;

        int consoleX = formLocation.X + formSize.Width;
        int consoleY = formLocation.Y + formSize.Height;

        Win32Interop.SetWindowPos(_consoleWindowHandlePtr, consoleX, consoleY);
    }

    /// <summary>
    /// Toggles the visibility of the console window.
    /// </summary>
    private void ConsoleWindowToggler()
    {
        consoleWindowTogglerButton.Enabled = false;

        if (_consoleWindowIsHidden)
        {
            ShowConsoleWindow();
        }
        else
        {
            HideConsoleWindow();
        }

        SetConsoleWindowTogglerButtonLabel();
        consoleWindowTogglerButton.Enabled = true;
    }
    #endregion Console Window Management Functions

    #region Event Handlers
    private async void AspNetCoreHostTogglerButton_Click(object sender, EventArgs e)
    {
        await AspNetCoreHostToggler();
    }

    private void ConsoleWindowTogglerButton_Click(object sender, EventArgs e)
    {
        ConsoleWindowToggler();
    }

    private void UrlLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        var url = e.Link?.LinkData as string;
        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception) { }
        }
    }

    /// <summary>
    /// Handles the form closing event by ensuring all resources are cleaned up.
    /// Prevents the form from closing until the ASP.NET Core host has fully stopped.
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs eventArgs)
    {
        // Second close attempt after cleanup
        if (_isFormClosing)
        {
            ResetConsoleWindowState();
            base.OnFormClosing(eventArgs);
            return;
        }

        // First close attempt - start cleanup
        if (_aspNetCoreAppHostIsRunning && _webHost != null)
        {
            // Cancel the close temporarily
            eventArgs.Cancel = true;

            // Cleanup
            Task.Run(async () =>
            {
                _isFormClosing = true;
                await StopAspNetCoreHost();

                // Now close the form
                BeginInvoke(Close);
            });
        }
        else
        {
            _isFormClosing = true;
            ResetConsoleWindowState();
            base.OnFormClosing(eventArgs);
        }
    }
    #endregion Event Handlers
}
