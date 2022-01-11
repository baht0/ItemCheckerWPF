using System;
using ItemChecker.Net;
using System.Diagnostics;
using System.Windows;
using System.IO;
using ItemChecker.Properties;
using ItemChecker.Services;
using Newtonsoft.Json.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace ItemChecker.MVVM.Model
{
    public class ProjectInfoService : BaseService
    {
        public static void AppUpdate()
        {
            if (BaseModel.token.IsCancellationRequested)
                return;
            if (StartUpProperties.Default.completionUpdate)
            {
                if (Directory.Exists(BaseModel.AppPath + "\\update\\ItemChecker"))
                {
                    DirectoryInfo info = new(BaseModel.AppPath + "\\update\\ItemChecker");
                    FileInfo[] Files = info.GetFiles();
                    foreach (FileInfo file in Files)
                    {
                        if (file.Name.Contains("ItemChecker.Updater"))
                        {
                            string newPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + file.Name;
                            File.Move(file.FullName, newPath, true);
                        }
                    }
                    info.Delete(true);
                }
                SettingsProperties.Default.Upgrade();
                HomeProperties.Default.Upgrade();
                ParserProperties.Default.Upgrade();
                FloatProperties.Default.Upgrade();

                StartUpProperties.Default.completionUpdate = false;
                StartUpProperties.Default.Save();
            }
            CheckUpdate();
        }
        public static void CheckUpdate()
        {
            JObject json = JObject.Parse(Post.DropboxRead("Updates/Version.json"));

            DataProjectInfo.LatestVersion = (string)json["LatestVersion"];
            DataProjectInfo.IsUpdate = DataProjectInfo.LatestVersion != DataProjectInfo.CurrentVersion;
        }
        public static void Update()
        {
            MessageBoxResult result = MessageBox.Show($"Want to upgrade from {DataProjectInfo.CurrentVersion} to {DataProjectInfo.LatestVersion}?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            ProcessStartInfo updater = new();
            updater.FileName = AppPath + "ItemChecker.Updater.exe";
            updater.Arguments = "1";
            Process.Start(updater);

            BrowserExit();
            Application.Current.Shutdown();
        }

        public static Boolean CreateCurrentVersion()
        {
            try
            {
                Post.DropboxDelete("ItemChecker");
                Thread.Sleep(200);
                Post.DropboxFolder("ItemChecker");
                foreach (var file in DataProjectInfo.FilesList)
                {
                    Thread.Sleep(200);
                    Post.DropboxUploadFile("ItemChecker/" + file, AppPath + file);
                }
                JObject json = new(
                    new JProperty("LatestVersion", DataProjectInfo.CurrentVersion),
                    new JProperty("Updated", DateTime.Now));

                Post.DropboxDelete("Updates/Version.json");
                Thread.Sleep(200);
                Post.DropboxUpload("Updates/Version.json", json.ToString(Formatting.None));

                return true;
            }
            catch (Exception ex)
            {
                errorLog(ex);
                return false;
            }
        }
    }
}