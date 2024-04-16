using System.Text.RegularExpressions;

class HttpRequest
{
    private string Request { get; set; }
    public string Method { get; set; }
    public string Path { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public string? Body { get; set; }

    public HttpRequest(string request)
    {
        Request = request;
        Parse();
    }

    private void Parse()
    {
        ParseStartLine();
        ParseHeaders();
        ParseBody();
    }

    private void ParseStartLine()
    {
        string pattern = @"^(\w+)\s+([^\s]+)\s+HTTP.*$";

        Match match = Regex.Match(Request, pattern, RegexOptions.Multiline);

        if (match.Success)
        {
            Method = match.Groups[1].Value.Trim().ToUpper();
            Path = match.Groups[2].Value.Trim();
        }
        else
        {
            throw new ArgumentException("Malformed HTTP start line");
        }
    }

    private void ParseBody()
    {
        string pattern = @"(?:\r?\n){2}([\s\S]*)";

        Match match = Regex.Match(Request, pattern, RegexOptions.Multiline);

        if (match.Success)
        {
            Body = match.Groups[1].Value.Trim();
        }
        else
        {
            Body = null;
        }
    }

    private void ParseHeaders()
    {
        string pattern = @"(?:\r?\n)([\s\S]*?)(?=\r?\n\r?\n)";

        Match match = Regex.Match(Request, pattern, RegexOptions.Multiline);

        if (match.Success && (match.Groups[1].Value != string.Empty))
        {
            var headers = match.Groups[1].Value.Split("\r\n");

            foreach (string pair in headers)
            {
                string key = pair.Split(":")[0].Trim();
                string value = pair.Split(":")[1].Trim();
                Headers.Add(key, value);
            }
        }
    }
}