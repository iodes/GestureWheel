using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GestureWheel.Managers;
using GestureWheel.Supports;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
                ToolTipText = "GestureWheel",
                ContextMenu = new ContextMenu(),
                Icon = Icon.ExtractAssociatedIcon(EnvironmentSupport.Executable)
            };

            _taskbarIcon.TrayMouseDoubleClick += delegate { ShowWithActivate(); };

            _taskbarIcon.ContextMenu.Items.Add(CreateMenuItem("프로그램 설정",
                ShowWithActivate, SymbolRegular.Wrench20));

            _taskbarIcon.ContextMenu.Items.Add(CreateMenuItem("업데이트 확인",
                () => Process.Start("https://github.com/iodes/GestureWheel"), SymbolRegular.Earth20));

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

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _mutex = new Mutex(true, $"{nameof(GestureWheel)}.App.Mutex", out bool createdNew);

            if (!createdNew)
            {
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
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            _mainWindow?.Close();
            GestureSupport.Stop();

            if (_taskbarIcon is null)
                return;

            _taskbarIcon.IsEnabled = false;
            _taskbarIcon.Dispose();
        }
    }
}
