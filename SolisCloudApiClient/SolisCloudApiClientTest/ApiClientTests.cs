using System.Reflection;
using SolisCloudApiClient;

namespace SolisCloudApiClientTest;

public class ApiClientTests
{
    private readonly ApiClient client;

    public ApiClientTests()
    {
        var solutionFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

        while (solutionFolder != null && solutionFolder.GetFiles("*.sln").Length == 0)
            solutionFolder = solutionFolder.Parent;

        if (solutionFolder != null)
        {
            var settings = File.ReadAllLines(solutionFolder.GetFiles("apiSettings.txt").Single().FullName);
            var key = settings[0];
            var secret = settings[1];
            client = new ApiClient(key, secret);
        }
    }

    [Test]
    public async Task UserStationList()
    {
        var userStationList =
            await client.Post<ListResponse<UserStation>>("/v1/api/userStationList", new UserStationListRequest(1, 10));

        foreach (var record in userStationList.data.page.records)
        {
            Console.WriteLine(record);
            Console.WriteLine();
        }

        var inverterList =
            await client.Post<ListResponse<UserStation>>("/v1/api/inverterList", new UserStationListRequest(1, 10));
    }

    [Test]
    public async Task InverterDay()
    {
        var inverterDay =
            await client.Post<InverterDayResponse>("/v1/api/inverterDay",
                new InverterDayRequest("6031023227030011", "2024-06-17", 0));

        foreach (var data in inverterDay.data)
        {
            var date = DateTime.SpecifyKind(DateTime.Parse(data.timeStr), DateTimeKind.Utc).ToLocalTime();
            var power = data.pac * double.Parse(data.pacPec);
            Console.WriteLine($"{date:G} : {data.pac,5:#,#} : {power,5:0.000}{data.pacStr}");
        }
    }

    private record UserStationListRequest(int pageNo, int pageSize);

    private record Response<T>(string code, T data);

    private record ListResponse<T>(string code, Data<T> data);

    private record Data<T>(Page<T> page);

    private record Page<T>(int mpptSwitch, int current, bool optimizeCountSql, int pages, List<T> records);

    private record UserStation(string id, string installer, string installerId, double dayEnergy,
        double gridPurchasedTodayEnergy, double gridSellTodayEnergy, double homeLoadTodayEnergy, double power);

    private record StationDetailRequest(string id);

    private record InverterDayRequest(string sn, string time, int timeZone);

    private record InverterDayResponse(string code, List<InverterDayData> data);

    private record InverterDayData(string dataTimestamp, double inverterTemperature, string time, string timeStr,
        int timeZone, double pac, string pacPec, string pacStr);
}