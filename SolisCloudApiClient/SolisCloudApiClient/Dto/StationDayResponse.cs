namespace SolisCloudApiClient.Dto
{
    internal record StationDayResponse(bool Success, string Code, string Msg, List<StationDayData> Data);
}
