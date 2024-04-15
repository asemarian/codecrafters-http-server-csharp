using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

internal class Program
{
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
        HttpResponseBuilder response = new();

        if (path.StartsWith("/echo/"))
        {
            string echoValue = ParseEchoPath(path);
    
            response.SetStatusCode(HttpStatusCode.OK);
            response.SetHeader("Content-Type", "text/plain");
            response.SetHeader("Content-Length", (echoValue.Length).ToString());
            response.SetBody(echoValue);
        }
        else if (path == "/")
        {
            response.SetStatusCode(HttpStatusCode.OK);
        }
        else
        {
            response.SetStatusCode(HttpStatusCode.NotFound);
        }

        stream.Write(System.Text.Encoding.ASCII.GetBytes(response.GetResponse()));
    }

    private static string ParseEchoPath(string path)
    {
        Match result = Regex.Match(path, @"^.*\/echo\/([^\s]+).*$", RegexOptions.Multiline);

        if (result.Success)
        {
            return result.Groups[1].Value;
        }

        return "";
    }
}