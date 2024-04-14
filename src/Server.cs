using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

internal class Program
{
    const string OK = "HTTP/1.1 200 OK\r\n\r\n";
    const string NotFound = "HTTP/1.1 404 Not Found\r\n\r\n";

    private static void Main(string[] args)
    {
        try
        {
            // bind to a tcp port
            TcpListener server = new TcpListener(IPAddress.Any, 4221);
            server.Start();

            // listen for incoming connection requests
            while (true)
            {
                using var client = server.AcceptTcpClient();

                Console.WriteLine("Client accepted!");

                var stream = client.GetStream();
                byte[] buffer = new byte[256];

                stream.Read(buffer, 0, buffer.Length);

                var request = System.Text.Encoding.ASCII.GetString(buffer);

                string? path = GetPath(request);

                RespondToClient(path, stream);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"SocketException: {e}");
        }

    }
    private static string GetPath(string input)
    {
        string pattern = @"^\w+\s+([^\s]+)\s+HTTP/.*$";
        Match match = Regex.Match(input, pattern, RegexOptions.Multiline);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        throw new ArgumentException("HTTP request is not valid");
    }

    private static void RespondToClient(string path, NetworkStream stream)
    {
        switch (path)
        {
            case "/":
                stream.Write(System.Text.Encoding.ASCII.GetBytes(OK));
                break;
            default:
                stream.Write(System.Text.Encoding.ASCII.GetBytes(NotFound));
                break;
        }
    }
}