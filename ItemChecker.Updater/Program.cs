using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ItemChecker.Updater
{
    class Program
    {
        protected static string updateFolder = AppDomain.CurrentDomain.BaseDirectory + @"\update";
        protected static DirectoryInfo dirInfo = new(updateFolder);
        static void Main(string[] args)
        {
            try
            {
                if (args.FirstOrDefault() != "1")
                    return;

                while (Process.GetProcessesByName("ItemChecker").Any())
                    Thread.Sleep(100);
                Console.WriteLine("Updating ItemChecker...");

                DirectoryInfo info = new(updateFolder + "\\ItemChecker\\");
                FileInfo[] Files = info.GetFiles();

                foreach (var file in Files)
                {
                    string newPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + file.Name;
                    File.Move(file.FullName, newPath, true);
                }
                Directory.Delete(updateFolder, true);
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\ItemChecker.exe");
            }
            catch (Exception exp)
            {
                Console.WriteLine("=======================================================");
                Console.WriteLine("\n\n" + exp.Message + "\n\n");

                Console.WriteLine("\nPress any key to proceed...");
                Console.ReadKey();
            }
        }
    }
}
