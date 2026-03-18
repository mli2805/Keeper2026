using System;

namespace KeeperWpf;

public class MyResponse
{
    public string Content { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;

    public MyResponse(string content)
    {
        Content = content;
        Success = true;
    }

    public MyResponse(Exception e)
    {
        Success = false;
        Error = e.Message;
    }
}
