namespace SolisCloudApiClient.Dto
{
    internal record StationMonthResponse(bool Success, string Code, string Msg, List<StationMonthData> Data);
}
