using System;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Newtonsoft.Json.Linq;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;

namespace ItemChecker.MVVM.Model
{
    public class ProjectInfoService : ProjectInfo
    {
        static DirectoryInfo DirInfo { get; set; } = new(UpdateFolder);
        public static bool AppCheck()
        {
            if (MainProperties.Default.CompletionUpdate)
            {
                SettingsProperties.Default.Upgrade();
                HomeProperties.Default.Upgrade();
                RareProperties.Default.Upgrade();
                MainProperties.Default.Upgrade();
            }
            return CheckUpdate();
        }
        static bool CheckUpdate()
        {
            JArray json = JArray.Parse(DropboxRequest.Get.Read("Updates.json"));

            DataProjectInfo.LatestVersion = (string)json.LastOrDefault()["version"];
            int latest = Convert.ToInt32(DataProjectInfo.LatestVersion.Replace(".", string.Empty));
            int current = Convert.ToInt32(DataProjectInfo.CurrentVersion.Replace(".", string.Empty));
            return latest > current;
        }
        public static void Update()
        {
            string pathZip = UpdateFolder + "\\ItemChecker.zip";

            if (Directory.Exists(UpdateFolder))
                DirInfo.Delete(true);
            DirInfo.Create();
            DirInfo.Attributes = FileAttributes.Hidden;

            DropboxRequest.Post.DownloadZip(pathZip);

            ZipFile.ExtractToDirectory(pathZip, UpdateFolder);

            DirectoryInfo info = new(UpdateFolder + "\\ItemChecker\\");
            FileInfo[] Files = info.GetFiles();
            foreach (FileInfo file in Files)
            {
                if (file.Name.Contains("ItemChecker.Updater"))
                {
                    string newPath = $"{AppPath}\\{file.Name}";
                    File.Move(file.FullName, newPath, true);
                }
            }
            ProcessStartInfo updater = new()
            {
                FileName = AppPath + "ItemChecker.Updater.exe",
                Arguments = "1"
            };
            Process.Start(updater);

            Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
        }

        public static bool UploadCurrentVersion()
        {
            try
            {
                string text = BaseService.OpenFileDialog("txt");
                if (!String.IsNullOrEmpty(text))
                {
                    JArray updates = JArray.Parse(DropboxRequest.Get.Read("Updates.json"));
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
                    DropboxRequest.Post.Delete("Updates.json");
                    Thread.Sleep(200);
                    DropboxRequest.Post.Upload("Updates.json", updates.ToString());
                }

                DropboxRequest.Post.Delete("ItemChecker");
                Thread.Sleep(200);
                DropboxRequest.Post.Folder("ItemChecker");
                foreach (var file in DataProjectInfo.FilesList)
                {
                    Thread.Sleep(200);
                    DropboxRequest.Post.UploadFile("ItemChecker/" + file, AppPath + file);
                }
                return true;
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex, true);
                return false;
            }
        }
    }
}