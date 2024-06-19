using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SolisCloudApiClient;

public class ApiClient
{
    private readonly HttpClient client = new();
    private readonly string key;
    private readonly string secret;

    public ApiClient(string key, string secret)
    {
        this.key = key;
        this.secret = secret;
        client.BaseAddress = new Uri("https://www.soliscloud.com:13333");
    }

    public async Task<T?> Post<T>(string url, object body)
    {
        var content = JsonSerializer.Serialize(body);
        var response = await Post(url, content);
        return JsonSerializer.Deserialize<T>(response);
    }

    private async Task<string> Post(string url, string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var date = DateTime.UtcNow.ToString("ddd, d MMM yyyy HH:mm:ss 'GMT'");

        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;charset=UTF-8");

        var contentMd5 = Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(content)));

        var hmacSha1 = new HMACSHA1(Encoding.UTF8.GetBytes(secret));

        var param = $"POST\n{contentMd5}\napplication/json\n{date}\n{url}";

        var sign = Convert.ToBase64String(hmacSha1.ComputeHash(Encoding.UTF8.GetBytes(param)));

        var auth = $"API {key}:{sign}";

        request.Headers.Add("Authorization", auth);
        request.Content.Headers.Add("Content-MD5", contentMd5);
        request.Headers.Add("Date", date);

        var result = await client.SendAsync(request);

        result.EnsureSuccessStatusCode();

        return await result.Content.ReadAsStringAsync();
    }
}