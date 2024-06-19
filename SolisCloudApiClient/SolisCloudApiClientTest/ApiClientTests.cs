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
        var userStationList = await client.UserStationList(1, 10);
        foreach (var userStation in userStationList)
        {
            Console.WriteLine(userStation);
            Console.WriteLine();
        }

        var inverterList = await client.InverterList(1, 10, null);
        foreach (var inverter in inverterList)
        {
            Console.WriteLine(inverter);
            Console.WriteLine();
        }
    }

    [Test]
    public async Task InverterDay()
    {
        var inverterDay =
            await client.Post<InverterDayResponse>("inverterDay",
                new InverterDayRequest("6031023227030011", "2024-06-17", 0));

        foreach (var data in inverterDay.data)
        {
            var date = DateTime.SpecifyKind(DateTime.Parse(data.timeStr), DateTimeKind.Utc).ToLocalTime();
            var power = data.pac * double.Parse(data.pacPec);
            Console.WriteLine($"{date:G} : {data.pac,5:#,#} : {power,5:0.000}{data.pacStr}");
        }
    }


    private record Response<T>(string code, T data);


    private record StationDetailRequest(string id);

    private record InverterDayRequest(string sn, string time, int timeZone);

    private record InverterDayResponse(string code, List<InverterDayData> data);

    private record InverterDayData(string dataTimestamp, double inverterTemperature, string time, string timeStr,
        int timeZone, double pac, string pacPec, string pacStr);
}