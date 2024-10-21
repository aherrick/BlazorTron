using ElectronNET.API;
using ElectronNET.API.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<BlazorTron.Components.App>().AddInteractiveServerRenderMode();

/// Electron Fun ///

await app.StartAsync();

var menu = new MenuItem[]
{
    new()
    {
        Label = "File",
        Type = MenuType.submenu,
        Submenu =
        [
            new MenuItem { Role = MenuRole.about },
            new MenuItem { Type = MenuType.separator },
            new MenuItem { Role = MenuRole.quit }
        ]
    },
};

Electron.Menu.SetApplicationMenu(menu);

var browserWindow = await Electron.WindowManager.CreateWindowAsync(
    new BrowserWindowOptions
    {
        Width = 800,
        Height = 600,
        Show = false
    }
);

await browserWindow.WebContents.Session.ClearCacheAsync();

browserWindow.OnMinimize += () =>
{
    MinimizeNotification();

    browserWindow.Hide();
};

// setup tray
await Electron.Tray.Show(
    Path.Combine(Environment.CurrentDirectory, "./wwwroot/favicon.png"),
    [
        .. new List<MenuItem>
        {
            new() { Label = "Show", Click = browserWindow.Show },
            new() { Label = "Exit", Click = browserWindow.Close }
        }
    ]
);

await Electron.Tray.SetToolTip("BlazorTron!");

Electron.Tray.OnDoubleClick += (_, _) =>
{
    browserWindow.Show();
};

Electron.App.SetLoginItemSettings(
    new LoginSettings
    {
        OpenAtLogin = true /*, OpenAsHidden = true,*/
    }
);

MinimizeNotification();

static void MinimizeNotification()
{
    Electron.Tray.DisplayBalloon(
        new DisplayBalloonOptions() { Title = "BlazorTron!", Content = "Chilling in your tray..." }
    );
}

app.WaitForShutdown();