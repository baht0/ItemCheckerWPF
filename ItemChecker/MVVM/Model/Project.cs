using ItemChecker.Net;
using ItemChecker.Properties;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class ProjectInfo
    {
        public static string AppPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
        public static string UpdateFolder
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"\update";
            }
        }
        public static string DocumentPath
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ItemChecker\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }
        public static string Theme
        {
            get
            {

                RegistryKey registry = Registry.CurrentUser.OpenSubKey(
                        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if ((int)registry.GetValue("SystemUsesLightTheme") == 1)
                    return "Light";
                else
                    return "Dark";
            }
        }
    }
    public class ProjectUpdates
    {
        public string Version { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
    public class DataProjectInfo
    {
        public static List<string> FilesList
        {
            get
            {
                return new()
                {
                    "ItemChecker.exe",
                    "ItemChecker.dll",
                    "ItemChecker.runtimeconfig.json",
                    "ItemChecker.Net.dll",
                    "ItemChecker.Support.dll",
                    "ItemChecker.Updater.exe",
                    "ItemChecker.Updater.dll",
                    "ItemChecker.Updater.runtimeconfig.json",
                    "icon.ico",
                    "Newtonsoft.Json.dll",
                    "HtmlAgilityPack.dll",
                    "MaterialDesignColors.dll",
                    "MaterialDesignThemes.Wpf.dll",
                };
            }
        }
        public static string CurrentVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        public static string LatestVersion { get; set; } = string.Empty;
    }
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
                string text = OpenFileDialog();
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
                BaseModel.ErrorLog(ex, true);
                return false;
            }
        }
        static string OpenFileDialog()
        {
            OpenFileDialog dialog = new()
            {
                InitialDirectory = ProjectInfo.DocumentPath,
                RestoreDirectory = true,
                Filter = $"File | *.txt"
            };

            return dialog.ShowDialog() == true ? File.ReadAllText(dialog.FileName) : string.Empty;
        }
    }
}
