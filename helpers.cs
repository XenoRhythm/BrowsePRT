namespace BrowsePRT;

public static class Helpers
{
    public static string GetBrowserCoreFilepath()
    {
        List<string> filelocs = [
            @"C:\Program Files\Windows Security\BrowserCore\browsercore.exe",
            @"C:\Windows\BrowserCore\browsercore.exe"
        ];

        return filelocs.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException("Could not find browsercore.exe");
    }
}