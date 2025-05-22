using SolisCloudApiClient.Domain;

namespace SolisCloudApiClient.Dto
{
    internal record StationYearData(string Id, double Money, string MoneyStr, string MoneyPec, double Energy, string EnergyStr, string EnergyPec, double FullHour,
        long Date, string DateStr, int TimeZone,
        double BatteryDischargeEnergy, double BatteryChargeEnergy, double GridPurchasedEnergy, double GridPurchasedIncome, double GridSellEnergy, double GridSellIncome,
        double HomeLoadEnergy, double ConsumeEnergy, double ProduceEnergy, double OffSetEnergy, double OffSetIncome, int ErrorFlag) : IStationData;
}
