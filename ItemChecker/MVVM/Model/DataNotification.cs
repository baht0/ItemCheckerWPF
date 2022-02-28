using System;

namespace ItemChecker.MVVM.Model
{
    public class DataNotification
    {
        public bool IsRead { get; set; } = false;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
