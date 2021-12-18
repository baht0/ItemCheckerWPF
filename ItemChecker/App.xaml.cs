using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Drawing;
using Forms = System.Windows.Forms;
using Model = ItemChecker.MVVM.Model;
using ItemChecker.MVVM.ViewModel;
using ItemChecker.MVVM.View;
using ItemChecker.Support;
using ItemChecker.Properties;

namespace ItemChecker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Forms.NotifyIcon notifyIcon;

        public App()
        {
            notifyIcon = new();
        }
        public ResourceDictionary ThemeDictionary => Resources.MergedDictionaries[0];

        public void ChangeTheme(Uri uri)
        {
            string current = ThemeDictionary.MergedDictionaries[0].Source.ToString();

            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            notifyIcon.Icon = new Icon(@"icon.ico");
            notifyIcon.MouseDoubleClick += notifyIconMouseDoubleClick;

            notifyIcon.ContextMenuStrip = new();
            notifyIcon.ContextMenuStrip.Items.Add("Setting", null, SettingClicked);
            notifyIcon.ContextMenuStrip.Items.Add(new Forms.ToolStripDropDownButton("Links", null,
                new Forms.ToolStripButton("TrySkins", null, OpenTrySkins),
                new Forms.ToolStripSeparator(),
                new Forms.ToolStripButton("Cs.Money", null, OpenCsm),
                new Forms.ToolStripSeparator(),
                new Forms.ToolStripButton("Inventory", null, OpenInventory),
                new Forms.ToolStripButton("SteamMarket", null, OpenStm)));
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, ExitClicked);

            notifyIcon.Visible = true;
            base.OnStartup(e);
        }
        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            if (GeneralProperties.Default.SetHours)
            {
                var turnOn = DateTime.Today.Date + GeneralProperties.Default.TurnOn.TimeOfDay;
                var turnOff = (DateTime.Now.Date + GeneralProperties.Default.TurnOff.TimeOfDay).AddDays(1);
                if (turnOn < DateTime.Now & turnOff > DateTime.Now)
                {
                    Model.BaseModel.Theme = "Dark";
                    ChangeTheme(new("/Themes/Dark.xaml", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    Model.BaseModel.Theme = "Light";
                    ChangeTheme(new("/Themes/Light.xaml", UriKind.RelativeOrAbsolute));
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
            if (MainWindow.DataContext is StartUpViewModel startVM)
                startVM.ExitCommand.Execute(null);
            if (MainWindow.DataContext is MainViewModel mainVM)
                mainVM.ExitCommand.Execute(null);
        }
        void SettingClicked(object sender, EventArgs e)
        {
            Window window = new SettingWindow();
            window.ShowDialog();
        }
        void OpenTrySkins(object sender, EventArgs e)
        {
            Edit.openUrl("https://table.altskins.com/");
        }
        void OpenCsm(object sender, EventArgs e)
        {
            Edit.openUrl("https://cs.money/");
        }
        void OpenInventory(object sender, EventArgs e)
        {
            Edit.openUrl("https://steamcommunity.com/my/inventory/");
        }
        void OpenStm(object sender, EventArgs e)
        {
            Edit.openUrl("https://steamcommunity.com/market/");
        }
    }
}