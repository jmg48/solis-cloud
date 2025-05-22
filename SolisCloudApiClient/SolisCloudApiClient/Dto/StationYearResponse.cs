namespace SolisCloudApiClient.Dto
{
    internal record StationYearResponse(bool Success, string Code, string Msg, List<StationYearData> Data);
}
