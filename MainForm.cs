using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

namespace WinFormsAspNetCoreSingleExe;

public partial class MainForm : Form
{
    private readonly IntPtr _consoleWindowHandlePtr;
    private bool _consoleWindowIsHidden;
    private bool _consoleCloseButtonIsDisabled;

    private bool _aspNetCoreAppHostIsRunning = false;

    private IHost? _webHost;
    private Task? _webHostTask = null;

    private bool _isFormClosing = false;
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

    private void StartAspNetCoreHost()
    {
        InitializeAspNetCoreHost();
        _webHostTask = _webHost!.StartAsync();
    }

    private async void StopAspNetCoreHost()
    {
        if (_webHost == null) return;

        _ = _webHost!.StopAsync(); // not awaiting here
        try
        {
            await _webHostTask!;
        }
        finally
        {
            _webHost.Dispose();
            _webHost = null;
        }
    }

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

    private void AspNetCoreHostToggler()
    {
        aspNetCoreHostTogglerButton.Enabled = false;

        if (_aspNetCoreAppHostIsRunning)
        {
            StopAspNetCoreHost();
            aspNetCoreHostTogglerButton.Text = "Start AspNetCore Host";
        }
        else
        {
            StartAspNetCoreHost();
            aspNetCoreHostTogglerButton.Text = "Stop AspNetCore Host";
        }

        // IHostApplicationLifetime Events will handle re-enabling aspNetCoreHostToggleButton 
    }
    #endregion UI Helpers

    #region Console Management Functions
    private void DisableConsoleCloseButton()
    {
        Win32Interop.DisableWindowCloseButton(_consoleWindowHandlePtr);
        _consoleCloseButtonIsDisabled = true;
    }

    private void EnableConsoleCloseButton()
    {
        Win32Interop.EnableWindowCloseButton(_consoleWindowHandlePtr);
        _consoleCloseButtonIsDisabled = false;
    }

    private void ShowConsoleWindow()
    {
        Win32Interop.RestoreWindowIfMinimized(_consoleWindowHandlePtr);
        SetConsolePositionRelativeToForm();
        Win32Interop.ShowWindow(_consoleWindowHandlePtr);
        _consoleWindowIsHidden = false;
    }
    private void HideConsoleWindow()
    {
        Win32Interop.HideWindow(_consoleWindowHandlePtr);
        _consoleWindowIsHidden = true;
    }

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

    private void SetConsolePositionRelativeToForm()
    {
        var formLocation = this.Location;
        var formSize = this.Size;

        int consoleX = formLocation.X + formSize.Width;
        int consoleY = formLocation.Y + formSize.Height;

        Win32Interop.SetWindowPos(_consoleWindowHandlePtr, consoleX, consoleY);
    }

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
    private void AspNetCoreHostTogglerButton_Click(object sender, EventArgs e)
    {
        AspNetCoreHostToggler();
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

    protected override void OnFormClosing(FormClosingEventArgs eventArgs)
    {
        _isFormClosing = true;

        ResetConsoleWindowState();

        if (_aspNetCoreAppHostIsRunning)
        {
            StopAspNetCoreHost();
        }

        base.OnFormClosing(eventArgs);
    }
    #endregion Event Handlers
}
