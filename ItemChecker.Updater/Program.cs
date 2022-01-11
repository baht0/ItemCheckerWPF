using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;

namespace ItemChecker.Updater
{
    class Program
    {
        protected static string path = AppDomain.CurrentDomain.BaseDirectory + @"\update";
        protected static DirectoryInfo dirInfo = new(path);
        static void Main(string[] args)
        {
            try
            {
                if (args.FirstOrDefault() != "1")
                    return;
                do
                    Thread.Sleep(1000);
                while (Process.GetProcessesByName("chromedriver").Any() | Process.GetProcessesByName("ItemChecker").Any());

                Console.WriteLine("Startup: success");

                if (Directory.Exists(path))
                    dirInfo.Delete(true);
                dirInfo.Create();
                dirInfo.Attributes = FileAttributes.Hidden;

                Console.WriteLine("\nDownloading...");
                DropboxDownloadZip();
                Console.WriteLine("Download: success");

                ZipFile.ExtractToDirectory(path + "\\ItemChecker.zip", path);
                Thread.Sleep(2000);
                DirectoryInfo info = new(path + "\\ItemChecker\\");
                do
                {
                    info = new(path + "\\ItemChecker\\");
                } while (info == null);
                FileInfo[] Files = info.GetFiles();

                foreach (var file in Files)
                {
                    if (!file.Name.Contains("ItemChecker.Updater"))
                    {
                        string newPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + file.Name;
                        File.Move(file.FullName, newPath, true);
                    }
                }
                Console.WriteLine("\nUpdate completed.");
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\ItemChecker.exe");
            }
            catch (Exception exp)
            {
                Console.WriteLine("\nSomething went wrong :(");
                Console.WriteLine("***\n" + exp.Message + "\n***");

                Console.WriteLine("\nPress any key to proceed...");
                Console.ReadKey();
            }
        }
        public static void DropboxDownloadZip()
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://content.dropboxapi.com/2/files/download_zip");

            httpRequest.Method = "POST";
            httpRequest.Headers["Authorization"] = "Bearer a94CSH6hwyUAAAAAAAAAAf3zRyhyZknI9J8KM3VZihWEILAuv6Vr3ht_-4RQcJxs";
            httpRequest.Headers["Dropbox-API-Arg"] = "{\"path\": \"/ItemChecker\"}";

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (Stream stream = httpResponse.GetResponseStream())
            {
                using (Stream zip = File.OpenWrite(path + @"\ItemChecker.zip"))
                {
                    stream.CopyTo(zip);
                }
            }
        }
    }
}
