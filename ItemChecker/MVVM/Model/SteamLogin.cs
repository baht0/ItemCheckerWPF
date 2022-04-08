namespace ItemChecker.MVVM.Model
{
    public class SteamLogin
    {
        public bool IsLoggedIn { get; set; } = false;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Remember { get; set; } = false;
        public string Code2AF { get; set; } = string.Empty;
    }
}
