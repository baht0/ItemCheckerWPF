using ItemChecker.MVVM.Model;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemChecker.Services
{
    public class FavoriteService : BaseService
    {
        public static void ExportTxt(ObservableCollection<string> FavoriteList)
        {
            string txt = string.Empty;
            foreach (string item in FavoriteList)
                txt += $"{item}\r\n";

            string path = $"{BaseModel.AppPath}\\extract";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(path + $"\\Home_Favorite_{DateTime.Now:dd.MM.yyyy_hh.mm}.txt", Edit.replaceSymbols(txt));
        }
        public ObservableCollection<string> ImportTxt()
        {
            List<string> items = OpenFileDialog("txt");
            return new ObservableCollection<string>(items);
        }
    }
}
