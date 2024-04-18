using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

using TcpListener server = new(IPAddress.Any, 4221);

try
{
    server.Start();

    while (true)
    {
        TcpClient client = await server.AcceptTcpClientAsync();
        _ = Task.Run(() => HandleClient(client));
    }
}
catch (SocketException e)
{
    Console.WriteLine($"SocketException: {e}");
}

void HandleClient(TcpClient client)
{
    var stream = client.GetStream();
    byte[] buffer = new byte[256];

    stream.Read(buffer, 0, buffer.Length);

    HttpRequest request = new(System.Text.Encoding.ASCII.GetString(buffer));

    // TODO: replace with a dedicated routing class
    RespondToClient(request, client);
}

void RespondToClient(HttpRequest request, TcpClient client)
{
    var stream = client.GetStream();
    var path = request.Path;
    HttpResponseBuilder response = new();

    if (path.StartsWith("/echo/"))
    {
        string echoValue = ParseEchoPath(path);

        response.SetStatusCode(HttpStatusCode.OK);
        response.SetHeader("Content-Type", "text/plain");
        response.SetHeader("Content-Length", echoValue.Length.ToString());
        response.SetBody(echoValue);
    }
    else if (path.StartsWith("/files/") && request.Method == "GET")
    {
        string directory = args[1];

        string fileName = ParseFileName(path);
        
        var filePath = Path.Combine([directory, fileName]);
        if (File.Exists(filePath))
        {
            response.SetStatusCode(HttpStatusCode.OK);
            response.SetHeader("Content-Type", "application/octet-stream");

            var file = File.ReadAllText(filePath);

            response.SetHeader("Content-Length", file.Length.ToString());
            response.SetBody(file);
        }
        else
        {
            response.SetStatusCode(HttpStatusCode.NotFound);
        }
    }
    else if (path.StartsWith("/files/") && request.Method == "POST")
    {
        string directory = args[1];

        string fileName = ParseFileName(path);

        var filePath = Path.Combine([directory, fileName]);
        // TODO: why do i have to trim the body each time?
        File.WriteAllText(filePath, request.Body?.Trim('\0'), System.Text.Encoding.ASCII);

        response.SetStatusCode(HttpStatusCode.Created);
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
    client.Close();
}

string ParseEchoPath(string path)
{
    Match result = Regex.Match(path, @"^.*\/echo\/([^\s]+).*$", RegexOptions.Multiline);

    if (result.Success)
    {
        return result.Groups[1].Value;
    }

    return "";
}

string ParseFileName(string path)
{
    Match result = Regex.Match(path, @"\/files\/(.*)", RegexOptions.Multiline);

    if (result.Success)
    {
        return result.Groups[1].Value;
    }

    return "";
}
