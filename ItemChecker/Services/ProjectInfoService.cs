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
        public static void AppCheck()
        {
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
            int latest = Convert.ToInt32(DataProjectInfo.LatestVersion.Replace(".", string.Empty));
            int current = Convert.ToInt32(DataProjectInfo.CurrentVersion.Replace(".", string.Empty));
            DataProjectInfo.IsUpdate = latest > current;
            if (DataProjectInfo.IsUpdate)
            {
                Main.Notifications.Add(new()
                {
                    Title = "Update available!",
                    Message = $"Latest version: {DataProjectInfo.LatestVersion}"
                });
                Main.Message.Enqueue("Update available!");
            }
        }
        public static void Update()
        {
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
                errorMessage(ex);
                return false;
            }
        }
    }
}