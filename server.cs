using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace BrowsePRT;

public sealed class Server : IDisposable
{
    public string PipeName { get; }
    public string? Nonce {get;set;}

    private NamedPipeServerStream? stdinPipe;
    private NamedPipeServerStream? stdoutPipe;

    public Server(string nonce)
    {
        PipeName = $"chrome.nativeMessaging.in{Convert.ToHexString(RandomNumberGenerator.GetBytes(8))}";
        Nonce = nonce;
    }

    public async Task StartAsync()
    {
        stdinPipe = new NamedPipeServerStream(
            PipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 2,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        Console.WriteLine($"Waiting for client on {PipeName}...");

        await stdinPipe.WaitForConnectionAsync();

        System.Console.WriteLine("Got a connection, asking for PRT...");

        //cmd i/o redirect brittle handling
        stdoutPipe = new NamedPipeServerStream(
            PipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 2,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        await stdoutPipe.WaitForConnectionAsync();

        string request = $"{{\"method\":\"GetCookies\",\"uri\":\"https://login.microsoftonline.com/common/oauth2/authorize?sso_nonce={Nonce}\",\"sender\":\"https://login.microsoftonline.com\"}}";
        byte[] requestBytes = Encoding.UTF8.GetBytes(request);
        byte[] requestLength = BitConverter.GetBytes(requestBytes.Length);
        
        await stdinPipe.WriteAsync(requestLength);
        await stdinPipe.WriteAsync(requestBytes);
        await stdinPipe.FlushAsync();

        byte[] responseLengthBytes = new byte[4];
        await stdoutPipe.ReadExactlyAsync(responseLengthBytes);
        int responseLength = BitConverter.ToInt32(responseLengthBytes);
        if (responseLength < 0)
        {
            throw new InvalidDataException("The response length cannot be negative.");
        }

        byte[] responseBytes = new byte[responseLength];
        await stdoutPipe.ReadExactlyAsync(responseBytes);
        Console.Write(Encoding.UTF8.GetString(responseBytes));
    }

    public void Dispose()
    {
        stdinPipe?.Dispose();
        stdoutPipe?.Dispose();
    }
}