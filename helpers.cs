namespace BrowsePRT
{
    public static class Helper
    {
        public static string GetBrowserCoreFilepath()
        {
            List<string> filelocs = new List<string>()
            {
                @"C:\Program Files\Windows Security\BrowserCore\browsercore.exe",
                @"C:\Windows\BrowserCore\browsercore.exe"
            };

            return filelocs.FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException("Could not find browsercore.exe");
        }
    }
}

