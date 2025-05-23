using SolisCloudApiClient.Domain;
using SolisCloudApiClient.Dto;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SolisCloudApiClient;

public class ApiClient
{
    private readonly ConcurrentDictionary<string, Task<StationAllResponse?>> stationAllCache = new();
    private readonly ConcurrentDictionary<(string, int), Task<StationYearResponse?>> stationYearCache = new();
    private readonly ConcurrentDictionary<(string, int, int), Task<StationMonthResponse?>> stationMonthCache = new();

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

    public async Task<IReadOnlyList<Station>> UserStationList(int pageNo = 1, int pageSize = 10)
    {
        var result =
            await Post<ListResponse<UserStation>>("userStationList", new UserStationListRequest(pageNo, pageSize));
        return (result?.data.page.records ?? []).Select(it => new Station(this, it)).ToList();
    }

    public async Task<IReadOnlyList<Inverter>> InverterList(int pageNo, int pageSize, int? stationId)
    {
        var result =
            await Post<ListResponse<Inverter>>("inverterList", new InverterListRequest(pageNo, pageSize, stationId));
        return result?.data.page.records ?? [];
    }

    public async Task<T?> Post<T>(string resource, object body)
    {
        var content = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var response = await Post($"/v1/api/{resource}", content);

        if (IsDebug)
        {
            Console.WriteLine(resource);
            Console.WriteLine();
            Console.WriteLine(response);
            Console.WriteLine();
        }

        return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public async Task<Dictionary<DateTime, IStationData>> StationAll(string stationId) => (await stationAllCache.GetOrAdd(
        stationId,
        it => Post<StationAllResponse>("stationAll", new StationAllRequest(stationId, "GBP", 0, null))))?.Data
        .ToDictionary(
            it => new DateTime(it.year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            it => it as IStationData)
        ?? [];

    public async Task<Dictionary<DateTime, IStationData>> StationYear(int year, string stationId) => (await stationYearCache.GetOrAdd(
        (stationId, year),
        it => Post<StationYearResponse>("stationYear", new StationYearRequest(it.Item1, "GBP", $"{it.Item2}", 0, null))))?.Data
        .ToDictionary(
            it => DateTime.UnixEpoch.AddMilliseconds(it.Date),
            it => it as IStationData)
        ?? [];

    public async Task<Dictionary<DateTime, IStationData>> StationMonth(int year, int month, string stationId) => (await stationMonthCache.GetOrAdd(
        (stationId, year, month),
        it => Post<StationMonthResponse>("stationMonth", new StationMonthRequest(it.Item1, "GBP", $"{it.Item2}-{it.Item3}", 0, null))))?.Data
        .ToDictionary(
            it => DateTime.UnixEpoch.AddMilliseconds(it.Date),
            it => it as IStationData)
        ?? [];

    public async Task<List<StationPower>> StationDay(DateTime day, string stationId) =>
        (await Post<StationDayResponse>("stationDay", new StationDayRequest(stationId, "GBP", $"{day:yyyy-MM-dd}", 0, null)))?
        .Data.Select(it => new StationPower(
            DateTime.UnixEpoch.AddMilliseconds(it.Time).AddHours(it.TimeZone),
            it.ProduceEnergy,
            it.BatteryPower,
            it.Psum,
            it.ConsumeEnergy)).ToList() ?? [];

    private async Task<string> Post(string url, string content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var date = DateTime.UtcNow.ToString("ddd, d MMM yyyy HH:mm:ss 'GMT'");

        request.Content = new StringContent(content);

        var contentMd5 = Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(content)));

        var hmacSha1 = new HMACSHA1(Encoding.UTF8.GetBytes(secret));

        var param = $"POST\n{contentMd5}\napplication/json\n{date}\n{url}";

        var sign = Convert.ToBase64String(hmacSha1.ComputeHash(Encoding.UTF8.GetBytes(param)));

        var auth = $"API {key}:{sign}";

        request.Content.Headers.Add("Content-MD5", contentMd5);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;charset=UTF-8");
        request.Headers.Add("Date", date);
        request.Headers.Add("Authorization", auth);

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