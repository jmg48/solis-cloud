using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolisCloudApiClient.Domain
{
    public class Station
    {
        private ApiClient apiClient;
        private UserStation userStation;

        public Station(ApiClient apiClient, UserStation userStation)
        {
            this.apiClient = apiClient;
            this.userStation = userStation;
        }

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
