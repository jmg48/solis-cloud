using SolisCloudApiClient.Domain;

namespace SolisCloudApiClient.Dto
{
    internal record StationDayData(
        double FamilyLoadPower,
        double BypassLoadPower,
        double BatteryPower,
        double BatteryPowerZheng,
        double BatteryPowerFu,
        double Psum,
        double PsumZheng,
        double PsumFu,
        double OneSelf,
        double ConsumeEnergy,
        double ProduceEnergy,
        long Time,
        string TimeStr,
        double Money,
        string MoneyStr,
        string MoneyPec,
        double Power,
        string PowerStr,
        string PowerPec,
        double GeneratorPower,
        double TimeZone
        );
}
