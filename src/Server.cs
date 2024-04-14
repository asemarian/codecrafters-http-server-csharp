using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

try
{
    // bind to a tcp port
    TcpListener server = new TcpListener(IPAddress.Any, 4221);
    server.Start();

    // prepare response message
    string response = "HTTP/1.1 200 OK\r\n\r\n";

    // listen for incoming connection requests
    while (true)
    {
        using var client = server.AcceptTcpClient();

        Console.WriteLine("Client accepted!");

        var stream = client.GetStream();
        byte[] byteReponse = System.Text.Encoding.ASCII.GetBytes(response);
        stream.Write(byteReponse);
    }
} catch (SocketException e)
{
    Console.WriteLine($"SocketException: {e}");
}