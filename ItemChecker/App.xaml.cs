using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Drawing;
using Forms = System.Windows.Forms;
using ItemChecker.MVVM.ViewModel;
using ItemChecker.MVVM.View;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ItemChecker.MVVM.Model;

namespace ItemChecker
{
    public partial class App : Application
    {
        public ResourceDictionary ThemeDictionary => Resources.MergedDictionaries[0];
        readonly Forms.NotifyIcon notifyIcon = new();

        public App()
        {
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }
        void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            switch (e.Category)
            {
                case UserPreferenceCategory.General:
                    ChangeTheme();
                    break;
            }
        }
        static bool ThemeIsLight()
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return (int)registry.GetValue("SystemUsesLightTheme") == 1;
        }
        public void ChangeTheme()
        {
            string theme = ThemeIsLight() ? "Light" : "Dark";
            Uri uri = new($"/Themes/{theme}.xaml", UriKind.RelativeOrAbsolute);
            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            notifyIcon.Icon = new Icon(@"icon.ico");
            notifyIcon.Text = "ItemChecker";
            notifyIcon.MouseDoubleClick += notifyIconMouseDoubleClick;

            notifyIcon.ContextMenuStrip = new();
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, ExitClicked);

            notifyIcon.Visible = true;
            base.OnStartup(e);
        }
        void ApplicationStart(object sender, StartupEventArgs e)
        {
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                MessageBox.Show("The program is already running!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Process.GetCurrentProcess().Kill();
            }

            ChangeTheme();
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Window start = new StartWindow();
            start.ShowDialog();

            if (start.DataContext is StartUpViewModel startVM)
            {
                if (StartUp.LaunchSuccessful)
                {
                    Window main = new MainWindow();
                    Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    Current.MainWindow = main;
                    main.Show();
                }
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            base.OnExit(e);
        }

        void notifyIconMouseDoubleClick(object sender, EventArgs e)
        {
            MainWindow.Visibility = Visibility.Visible;
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
        }
        void ExitClicked(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to close?", "Question",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Current.Shutdown();
        }
    }
}