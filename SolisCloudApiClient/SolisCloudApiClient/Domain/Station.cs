namespace SolisCloudApiClient.Domain
{
    public class Station(ApiClient apiClient, UserStation userStation)
    {
        public string Id => userStation.id;

        public async Task<Dictionary<DateTime, IStationData>> StationAll()
        {
            return await apiClient.StationAll(userStation.id);
        }

        public async Task<Dictionary<DateTime, IStationData>> StationYear(int year)
        {
            return await apiClient.StationYear(year, userStation.id);
        }
    }
}
