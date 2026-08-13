using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace BrowsePRT
{
    public sealed class Server : IDisposable
    {
        public string PipeName { get; }
        public string Nonce {get;set;}

        private NamedPipeServerStream stdinPipe;
        private NamedPipeServerStream stdoutPipe;

        public Server(string nonce)
        {
            byte[] bytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            PipeName = $"chrome.nativeMessaging.in{BitConverter.ToString(bytes).Replace("-", "")}";
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

            //cmd i/o redirect brittle handling
            stdoutPipe = new NamedPipeServerStream(
                PipeName,
                PipeDirection.InOut,
                maxNumberOfServerInstances: 2,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            Console.WriteLine($"Waiting for client on {PipeName}...");

            await stdinPipe.WaitForConnectionAsync();
            System.Console.WriteLine("Got stdin connection");

            await stdoutPipe.WaitForConnectionAsync();
            System.Console.WriteLine("Got stdout connection");

            System.Console.WriteLine("Requesting PRT");
            string request = $"{{\"method\":\"GetCookies\",\"uri\":\"https://login.microsoftonline.com/common/oauth2/authorize?sso_nonce={Nonce}\",\"sender\":\"https://login.microsoftonline.com\"}}";
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);
            byte[] requestLength = BitConverter.GetBytes(requestBytes.Length);
            await stdinPipe.WriteAsync(requestLength,0,requestLength.Length);
            await stdinPipe.WriteAsync(requestBytes,0,requestBytes.Length);
            await stdinPipe.FlushAsync();

            byte[] responseLengthBytes = new byte[4];
            await ReadExactlyAsync(stdoutPipe,responseLengthBytes);
            int responseLength = BitConverter.ToInt32(responseLengthBytes,0);
            if (responseLength < 0) throw new InvalidDataException("The response length cannot be negative.");

            byte[] responseBytes = new byte[responseLength];
            await ReadExactlyAsync(stdoutPipe,responseBytes);

            Console.Write(Encoding.UTF8.GetString(responseBytes));
        }

        private static async Task ReadExactlyAsync(Stream stream, byte[] buffer)
        {
            int offset = 0;

            while (offset < buffer.Length)
            {
                int bytesRead = await stream.ReadAsync(buffer, offset, buffer.Length - offset);

                if (bytesRead == 0)
                    throw new EndOfStreamException();

                offset += bytesRead;
            }
        }

        public void Dispose()
        {
            stdinPipe?.Dispose();
            stdoutPipe?.Dispose();
        }
    }
}

