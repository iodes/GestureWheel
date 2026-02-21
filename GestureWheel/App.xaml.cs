using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GestureWheel.Localization;
using GestureWheel.Managers;
using GestureWheel.Supports;
using GestureWheel.Utilities;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using MenuItem = Wpf.Ui.Controls.MenuItem;
using SymbolIcon = Wpf.Ui.Controls.SymbolIcon;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace GestureWheel
{
    public partial class App
    {
        #region Fields
        // ReSharper disable once NotAccessedField.Local
        private Mutex _mutex;
        private MainWindow _mainWindow;
        private TaskbarIcon _taskbarIcon;
        private MenuItem _menuSettings;
        private MenuItem _menuHomepage;
        private MenuItem _menuExit;
        #endregion

        #region Private Methods
        private void ShowWithActivate()
        {
            _mainWindow?.Show();
            _mainWindow?.Activate();
        }

        private void InitializeUserInterface()
        {
            _mainWindow = new MainWindow();

            _taskbarIcon = new TaskbarIcon
            {
                IsEnabled = true,
                ContextMenu = new ContextMenu(),
                Icon = Icon.ExtractAssociatedIcon(EnvironmentSupport.Executable)
            };

            _taskbarIcon.TrayMouseDoubleClick += delegate { ShowWithActivate(); };

            _menuSettings = CreateMenuItem(ShowWithActivate, SymbolRegular.Wrench20);
            _menuHomepage = CreateMenuItem(() => UrlUtility.Open("https://github.com/iodes/GestureWheel"), SymbolRegular.Open20);
            _menuExit = CreateMenuItem(() => Current.Shutdown(), SymbolRegular.ArrowExit20);

            _taskbarIcon.ContextMenu.Items.Add(_menuSettings);
            _taskbarIcon.ContextMenu.Items.Add(_menuHomepage);
            _taskbarIcon.ContextMenu.Items.Add(new Separator());
            _taskbarIcon.ContextMenu.Items.Add(_menuExit);

            UpdateTrayMenuHeaders();
            LocalizationManager.Instance.PropertyChanged += (_, _) => UpdateTrayMenuHeaders();
        }

        private void UpdateTrayMenuHeaders()
        {
            var loc = LocalizationManager.Instance;
            if (_menuSettings != null) _menuSettings.Header = loc["TraySettings"];
            if (_menuHomepage != null) _menuHomepage.Header = loc["TrayHomepage"];
            if (_menuExit != null) _menuExit.Header = loc["TrayExit"];
        }

        private MenuItem CreateMenuItem(Action action, SymbolRegular? icon = null)
        {
            var item = new MenuItem();

            if (icon is not null)
                item.Icon = new SymbolIcon(icon.Value);

            item.Click += delegate { action(); };
            return item;
        }
        #endregion

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is not Exception exception)
                return;

            Log.Error(exception, "An unknown error has occurred!");
            MessageBox.Show(exception.Message, $"{nameof(GestureWheel)}", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(EnvironmentSupport.Logs, "log-.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _mutex = new Mutex(true, $"{nameof(GestureWheel)}.App.Mutex", out bool createdNew);

            if (!createdNew)
            {
                Log.Warning("The app is already running. Terminates the current process!");
                Current.Shutdown();
                return;
            }

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            SettingsManager.Load();
            LocalizationManager.Instance.ApplyFromSettings(SettingsManager.Current.Language);
            SettingsManager.UpdateAutoStartup();
            InitializeUserInterface();

            GestureSupport.Start();

            if (Environment.GetCommandLineArgs().Contains("/Activate", StringComparer.OrdinalIgnoreCase))
                ShowWithActivate();

            Log.Information($"{nameof(GestureWheel)} {Assembly.GetExecutingAssembly().GetName().Version} started successfully");

            if (SettingsManager.Current.UseAutoUpdate)
            {
                try
                {
                    _ = UpdateSupport.CheckUpdateAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred during auto update checking!");
                }
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            _mainWindow?.Close();
            GestureSupport.Stop();

            if (_taskbarIcon is null)
                return;

            _taskbarIcon.IsEnabled = false;
            _taskbarIcon.Dispose();

            Log.Information($"{nameof(GestureWheel)} {Assembly.GetExecutingAssembly().GetName().Version} terminated successfully");
        }
    }
}
