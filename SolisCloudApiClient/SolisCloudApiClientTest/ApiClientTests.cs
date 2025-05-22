using NUnit.Framework.Internal;
using SolisCloudApiClient;
using System.ComponentModel.DataAnnotations;

namespace SolisCloudApiClientTest;

public class ApiClientTests
{
    private readonly ApiClient client = new();

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
    public async Task StationDay()
    {
        var station = (await client.UserStationList()).Single();

    }

    [Test]
    public async Task StationMonth()
    {
        var station = (await client.UserStationList()).Single();

        var yearly = (await station.StationAll())
            .Select(it =>
            {
                var year = it.Key;
                var yearData = it.Value;

                var energy = yearData.Energy;
                var earned = yearData.GridSellEnergy * ExportRate(year);
                var saved = (yearData.Energy - yearData.GridSellEnergy) * .3306;
                var rate = (earned + saved) / energy * 100;

                return (year, energy, earned, saved, rate);
            });

        var monthly = Enumerable.Range(2023, 3).ToAsyncEnumerable()
            .SelectManyAwait(async it => (await station.StationYear(it)).ToAsyncEnumerable())
            .Select(it =>
            {
                var month = it.Key;
                var monthData = it.Value;

                var energy = monthData.Energy;
                var earned = monthData.GridSellEnergy * ExportRate(month);
                var saved = (monthData.Energy - monthData.GridSellEnergy) * .3306;
                var rate = (earned + saved) / energy * 100;

                return (month, energy, earned, saved, rate);
            }).ToEnumerable();

        var totalIncome = yearly.Sum(it => it.earned + it.saved);
        Console.WriteLine($"totalIncome: {totalIncome:£#,###.00}");
        Console.WriteLine();

        foreach (var (year, energy, earned, saved, rate) in yearly.OrderBy(it => it.year))
        {
            Console.WriteLine($"year: {year:yyyy}, energy: {energy,5:0.0}, income: {earned + saved,7:£0.00}, earned: {earned,7:£0.00}, saved: {saved,7:£0.00}, rate: {rate,5:0.0p}");
        }

        Console.WriteLine();

        foreach (var (month, energy, earned, saved, rate) in monthly.OrderBy(it => it.month))
        {
            Console.WriteLine($"month: {month:MMM-yyyy}, energy: {energy,5:0.0}, income: {earned + saved,7:£0.00}, earned: {earned,7:£0.00}, saved: {saved,7:£0.00}, rate: {rate,5:0.0p}");
        }

        Console.WriteLine();
    }

    private double ExportRate(DateTime when)
    {
        if (when > new DateTime(2024, 10, 1))
        {
            return .1032;
        }

        if (when > new DateTime(2023, 10, 1))
        {
            return .1422;
        }

        if (when > new DateTime(2022, 10, 1))
        {
            return .1766;
        }

        throw new NotSupportedException();
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