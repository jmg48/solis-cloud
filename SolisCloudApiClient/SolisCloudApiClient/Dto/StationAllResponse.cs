namespace SolisCloudApiClient.Dto
{
    internal record StationAllResponse(bool Success, string Code, string Msg, List<StationAllData> Data);
}
