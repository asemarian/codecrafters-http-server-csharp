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
            using TcpListener server = new TcpListener(IPAddress.Any, 4221);
            server.Start();

            // listen for incoming connection requests
            while (true)
            {
                using var client = server.AcceptTcpClient();

                Console.WriteLine("Client accepted!");

                using var stream = client.GetStream();
                byte[] buffer = new byte[256];

                stream.Read(buffer, 0, buffer.Length);

                HttpRequest request = new(System.Text.Encoding.ASCII.GetString(buffer));

                // TODO: replace with a dedicated routing class
                RespondToClient(request, stream);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"SocketException: {e}");
        }
    }

    private static void RespondToClient(HttpRequest request, NetworkStream stream)
    {
        var path = request.Path;
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
        else if (path == "/user-agent" && request.Method == "GET")
        {
            response.SetHeader("Content-Type", "text/plain");
            response.SetHeader("Content-Length", request.Headers["User-Agent"].Length.ToString());
            response.SetStatusCode(HttpStatusCode.OK);
            response.SetBody(request.Headers["User-Agent"]);
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