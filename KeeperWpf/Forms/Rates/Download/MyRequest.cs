using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KeeperWpf;

public static class MyRequest
{
    private static readonly HttpClientHandler _handler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
        UseProxy = false // на компе настроен прокси в англию, и тогда нацбанк не отвечает, остальным сайтам без разницы
    };

    private static readonly HttpClient _httpClient = new HttpClient(_handler)
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    static MyRequest()
    {
        // без этого умеет читать только UTF-8
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static async Task<MyResponse> GetResponseAsync(string uri)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync();
                return new MyResponse(str);
            }
            else
            {
                return new MyResponse(new Exception($"HTTP Error: {response.StatusCode}"));
            }
        }
        catch (Exception e)
        {
            return new MyResponse(e);
        }
    }
}