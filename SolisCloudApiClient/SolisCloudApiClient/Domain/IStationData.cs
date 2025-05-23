namespace SolisCloudApiClient.Domain;

public interface IStationData {
    double Energy { get; }
    double GridSellEnergy { get; }
}
