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

    public ApiClient()
        : this(Environment.GetEnvironmentVariable("SOLIS_KEY_ID"), Environment.GetEnvironmentVariable("SOLIS_KEY_SECRET"))
    {        
    }

    public ApiClient(string? key, string? secret)
    {
        this.key = key!;
        this.secret = secret!;
        client.BaseAddress = new Uri("https://www.soliscloud.com:13333");
    }

    public bool IsDebug { get; set; }

    public async Task<IReadOnlyList<UserStation>> UserStationList(int pageNo, int pageSize)
    {
        var result =
            await Post<ListResponse<UserStation>>("userStationList", new UserStationListRequest(pageNo, pageSize));
        return result.data.page.records;
    }

    public async Task<IReadOnlyList<Inverter>> InverterList(int pageNo, int pageSize, int? stationId)
    {
        var result =
            await Post<ListResponse<Inverter>>("inverterList", new InverterListRequest(pageNo, pageSize, stationId));
        return result.data.page.records;
    }

    public async Task<T?> Post<T>(string resource, object body)
    {
        var content = JsonSerializer.Serialize(body);
        var response = await Post($"/v1/api/{resource}", content);

        if (IsDebug)
        {
            Console.WriteLine(resource);
            Console.WriteLine();
            Console.WriteLine(response);
            Console.WriteLine();
        }

        return JsonSerializer.Deserialize<T>(response);
    }

    private async Task<string> Post(string url, string content)
    {
        //Console.WriteLine($"url: {url}");
        //Console.WriteLine($"content: {content}");

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var date = DateTime.UtcNow.ToString("ddd, d MMM yyyy HH:mm:ss 'GMT'");

        request.Content = new StringContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;charset=UTF-8");

        var contentMd5 = Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(content)));

        var hmacSha1 = new HMACSHA1(Encoding.UTF8.GetBytes(secret));

        var param = $@"POST
{contentMd5}
application/json
{date}
{url}";

        var sign = Convert.ToBase64String(hmacSha1.ComputeHash(Encoding.UTF8.GetBytes(param)));

        var auth = $"API {key}:{sign}";

        request.Headers.Add("Time", date);
        request.Headers.Add("Authorization", auth);
        request.Headers.Add("User-Agent", "SolisCloudApiClient");
        request.Content.Headers.Add("Content-MD5", contentMd5);

        var result = await client.SendAsync(request);

        try
        {
            result.EnsureSuccessStatusCode();
        }
        catch(Exception ex)
        {
            throw new Exception(await result.Content.ReadAsStringAsync(), ex);
        }

        return await result.Content.ReadAsStringAsync();
    }
}

public record UserStationListRequest(int pageNo, int pageSize);

public record InverterListRequest(int pageNo, int pageSize, int? stationId);

public record ListResponse<T>(Data<T> data);

public record Data<T>(Page<T> page);

public record Page<T>(int current, int pages, List<T> records);

public record UserStation(string id, string installer, string installerId, double allEnergy1, double allIncome,
    double dayEnergy1, double dayIncome, double gridPurchasedTodayEnergy, double gridPurchasedTotalEnergy,
    double gridSellTodayEnergy, double gridSellTotalEnergy, double homeLoadTodayEnergy, double homeLoadTotalEnergy,
    double monthEnergy1, double power1, double yearEnergy1);

public record Inverter(string id, string collectorId, string collectorSn, string dataTimestamp, string dataTimestampStr,
    double etoday1, double etotal1, double familyLoadPower, double gridPurchasedTodayEnergy, double gridSellTodayEnergy,
    double homeLoadTodayEnergy, double pac1, double pow1, double pow2, double power1, double totalFullHour,
    double totalLoadPower);