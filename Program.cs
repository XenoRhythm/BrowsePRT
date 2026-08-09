//create named pipe
//start cmd with exe
//write to pipe
//read from pipe
//close process
//close pipe
using System.Diagnostics;
using BrowsePRT;

using (Process cmd = new Process())
{
        System.Console.WriteLine("Enter nonce: ");
        string nonce = Console.ReadLine()
            ?? throw new InvalidOperationException("Nonce is required.");

        using var srv = new Server(nonce);
        Task serverTask = srv.StartAsync();

        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.Arguments = $"/d /c \"{Helpers.GetBrowserCoreFilepath()}\" < \\\\.\\pipe\\{srv.PipeName} > \\\\.\\pipe\\{srv.PipeName}";
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        await serverTask;
        await cmd.WaitForExitAsync();
}