using System.Net;

class HttpResponseBuilder
{
    private string Delimiter { get; set; } = "\r\n";

    private string StartLine { get; set; } = "HTTP/1.1";

    private string Headers { get; set; } = "";

    private string Body { get; set; } = "";

    public HttpResponseBuilder()
    {
        Reset();
    }

    public void Reset()
    {
        Headers = Body = string.Empty;
    }

    public void SetStatusCode(HttpStatusCode statusCode)
    {
        // TODO: status text needs to be modified if it contains multiple words (e.g. Not Found instead of NotFound) 
        StartLine = $"{StartLine} {(int)statusCode} {statusCode}{Delimiter}";
    }

    public void SetHeader(string key, string value)
    {
        Headers += $"{key.Trim()}: {value.Trim()}{Delimiter}";
    }

    public void SetBody(string body)
    {
        Body = body;
    }

    public string GetResponse()
    {
        var result = $"{StartLine}{Headers}{Delimiter}{Body}";
        Reset();
        return result;
    }
}