using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemChecker.MVVM.Model
{
    public class Notification
    {
        public string Title { get; set; }
        public string Color { get; set; }
        public string Desc { get; set; }
        public DateTime Time { get; set; }
    }
}
