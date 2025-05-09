using NUnit.Framework.Internal;
using SolisCloudApiClient;
using System.ComponentModel.DataAnnotations;

namespace SolisCloudApiClientTest;

public class ApiClientTests
{
    private readonly ApiClient client = new ApiClient();

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
    public async Task StationMonth()
    {
        var monthly = new Dictionary<DateTime, (double, double)>();

        for (var month = new DateTime(2023, 1, 1); month < DateTime.Now; month = month.AddMonths(1))
        {
            await Task.Delay(100);

            var inverterDay =
                await client.Post<StationMonthResponse>("stationMonth",
                    new StationMonthRequest(1298491919448946467, "GBP", $"{month:yyyy-MM}", 0, null));

            var monthlyEnergy = 0.0;
            var monthlyIncome = 0.0;
            foreach (var day in inverterDay.data)
            {
                var date = DateTime.UnixEpoch.AddMilliseconds(day.date);

                var energy = day.energy;
                var exported = day.gridSellEnergy;
                var notImported = energy - exported;

                var income = exported * ExportRate(date) + notImported * .3306;

                // Console.WriteLine($"date: {date:dd-MMM-yyyy}, income: £{income:0.00}, energy: {day.energy:0.0}, imported: {day.gridPurchasedEnergy:0.0}, exported: {day.gridSellEnergy:0.0}");

                monthlyEnergy += energy;
                monthlyIncome += income;
            }

            monthly.Add(month, (monthlyEnergy, monthlyIncome));

            Console.WriteLine($"month: {month:MMM-yyyy}, energy: {monthlyEnergy,5:0.0}, income: {monthlyIncome,7:£0.00}");
        }

        Console.WriteLine();
        foreach (var group in monthly.GroupBy(it => it.Key.Year))
        {
            Console.WriteLine($"year: {group.Key}, energy: {group.Sum(it => it.Value.Item1),5:0.0}, income: {group.Sum(it => it.Value.Item2),7:£0.00}");
        }
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

    private record StationMonthRequest(long id, string money, string month, int timeZone, string nmiCode);

    private record StationMonthResponse(bool success, string code, string msg, List<StationMonthData> data);
    
    private record StationMonthData(string id, double money, string moneyStr, string moneyPec, double energy, string energyStr, string energyPec, double fullHour,
        long date, string dateStr, int timeZone,
        double batteryDischargeEnergy, double batteryChargeEnergy, double gridPurchasedEnergy, double gridPurchasedIncome, double gridSellEnergy, double gridSellIncome,
        double homeLoadEnergy, double consumeEnergy, double produceEnergy, double offSetEnergy, double offSetIncome, int errorFlag);

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