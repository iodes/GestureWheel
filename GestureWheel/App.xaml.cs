﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GestureWheel.Managers;
using GestureWheel.Supports;
using GestureWheel.Utilities;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Wpf.Ui.Common;
using MenuItem = Wpf.Ui.Controls.MenuItem;

namespace GestureWheel
{
    public partial class App
    {
        #region Fields
        // ReSharper disable once NotAccessedField.Local
        private Mutex _mutex;
        private MainWindow _mainWindow;
        private TaskbarIcon _taskbarIcon;
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

            _taskbarIcon.ContextMenu.Items.Add(CreateMenuItem("프로그램 설정",
                ShowWithActivate, SymbolRegular.Wrench20));

            _taskbarIcon.ContextMenu.Items.Add(CreateMenuItem("홈페이지 방문",
                () => UrlUtility.Open("https://github.com/iodes/GestureWheel"), SymbolRegular.Open20));

            _taskbarIcon.ContextMenu.Items.Add(new Separator());

            _taskbarIcon.ContextMenu.Items.Add(CreateMenuItem("종료",
                () => Current.Shutdown(), SymbolRegular.ArrowExit20));
        }

        private MenuItem CreateMenuItem(string header, Action action, SymbolRegular? icon = null)
        {
            var item = new MenuItem
            {
                Header = header
            };

            if (icon is not null)
                item.SymbolIcon = icon.Value;

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
