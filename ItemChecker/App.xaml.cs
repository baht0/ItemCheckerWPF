using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Drawing;
using Forms = System.Windows.Forms;
using Model = ItemChecker.MVVM.Model;
using ItemChecker.MVVM.ViewModel;
using ItemChecker.MVVM.View;
using ItemChecker.Properties;
using ItemChecker.MVVM.Model;
using System.IO;

namespace ItemChecker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Forms.NotifyIcon notifyIcon = new();

        public App()
        {

        }
        public ResourceDictionary ThemeDictionary => Resources.MergedDictionaries[0];

        public void ChangeTheme(Uri uri)
        {
            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
        }
        public void AutoChangeTheme()
        {
            var now = DateTime.Now;
            var turnOn = now.Date + SettingsProperties.Default.TurnOn.TimeOfDay;
            var turnOff = now.Date + SettingsProperties.Default.TurnOff.TimeOfDay;
            bool dark = turnOn < now & now < turnOff.AddDays(1);
            bool light = turnOn > now & now > turnOff;
            if (dark & Model.ProjectInfo.Theme == "Light")
            {
                Model.ProjectInfo.Theme = "Dark";
                ChangeTheme(new("/Themes/Dark.xaml", UriKind.RelativeOrAbsolute));
            }
            if (light & Model.ProjectInfo.Theme == "Dark")
            {
                Model.ProjectInfo.Theme = "Light";
                ChangeTheme(new("/Themes/Light.xaml", UriKind.RelativeOrAbsolute));
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            notifyIcon.Icon = new Icon(@"icon.ico");
            notifyIcon.Text = "ItemChecker";
            notifyIcon.MouseDoubleClick += notifyIconMouseDoubleClick;

            notifyIcon.ContextMenuStrip = new();
            notifyIcon.ContextMenuStrip.Items.Add("Logout", null, LogoutClicked);
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, ExitClicked);

            notifyIcon.Visible = true;
            base.OnStartup(e);
        }
        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            if (SettingsProperties.Default.SetHours)
                AutoChangeTheme();
            else
            {
                switch (SettingsProperties.Default.Theme)
                {
                    case "Light":
                        Model.ProjectInfo.Theme = "Light";
                        ChangeTheme(new("/Themes/Light.xaml", UriKind.RelativeOrAbsolute));
                        break;
                    case "Dark":
                        Model.ProjectInfo.Theme = "Dark";
                        ChangeTheme(new("/Themes/Dark.xaml", UriKind.RelativeOrAbsolute));
                        break;
                }
            }


            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Window start = new StartWindow();
            start.ShowDialog();

            if (start.DataContext is StartUpViewModel startVM)
            {
                if (startVM.LoginSuccessful)
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
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;
            else if (MainWindow.DataContext is StartUpViewModel startVM)
                startVM.ExitCommand.Execute(null);
            else if (MainWindow.DataContext is MainViewModel mainVM)
                mainVM.ExitCommand.Execute(null);
        }
        void LogoutClicked(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            MainProperties.Default.SteamLoginSecure = string.Empty;
            MainProperties.Default.SteamCurrencyId = 0;
            MainProperties.Default.SessionBuff = string.Empty;
            MainProperties.Default.Save();

            string profilesDir = ProjectInfo.DocumentPath + "profile";
            if (Directory.Exists(profilesDir))
                Directory.Delete(profilesDir);

            Application.Current.Shutdown();
        }
    }
}