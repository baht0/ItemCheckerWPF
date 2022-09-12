using System;
using ItemChecker.Net;
using System.Diagnostics;
using System.Windows;
using System.IO;
using ItemChecker.Properties;
using ItemChecker.Services;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ProjectInfoService : ProjectInfo
    {
        public static void AppCheck()
        {
            if (!Directory.Exists(DocumentPath))
                Directory.CreateDirectory(DocumentPath);
            if (StartUpProperties.Default.CompletionUpdate)
            {
                if (Directory.Exists(AppPath + "\\update\\ItemChecker"))
                {
                    DirectoryInfo info = new(AppPath + "\\update\\ItemChecker");
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
                RareProperties.Default.Upgrade();
                StartUpProperties.Default.Upgrade();

                StartUpProperties.Default.CompletionUpdate = false;
                StartUpProperties.Default.Save();
            }
            CheckUpdate();
        }
        public static void CheckUpdate()
        {
            JArray json = JArray.Parse(Get.DropboxRead("Updates.json"));

            DataProjectInfo.LatestVersion = (string)(json.LastOrDefault()["version"]);
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

            BaseService.BrowserExit();
            Application.Current.Shutdown();
        }

        public static Boolean UploadCurrentVersion()
        {
            try
            {
                string text = BaseService.OpenFileDialog("txt");
                if (!String.IsNullOrEmpty(text))
                {
                    //upload file
                    Post.DropboxDelete("ItemChecker");
                    Thread.Sleep(200);
                    Post.DropboxFolder("ItemChecker");
                    foreach (var file in DataProjectInfo.FilesList)
                    {
                        Thread.Sleep(200);
                        Post.DropboxUploadFile("ItemChecker/" + file, AppPath + file);
                    }
                    //ver file
                    JArray updates = JArray.Parse(Get.DropboxRead("Updates.json"));
                    JObject obj = (JObject)updates.FirstOrDefault(x => (string)x["version"] == DataProjectInfo.CurrentVersion);

                    if (obj != null)
                    {
                        int id = updates.IndexOf(obj);
                        updates[id]["text"] = text;
                    }
                    else
                    {
                        updates.Add(new JObject(
                                        new JProperty("date", DateTime.Now),
                                        new JProperty("version", DataProjectInfo.CurrentVersion),
                                        new JProperty("text", text)));
                    }

                    updates = new JArray(updates.OrderBy(obj => (DateTime)obj["date"]));
                    Post.DropboxDelete("Updates.json");
                    Thread.Sleep(200);
                    Post.DropboxUpload("Updates.json", updates.ToString());

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex, true);
                return false;
            }
        }
    }
}