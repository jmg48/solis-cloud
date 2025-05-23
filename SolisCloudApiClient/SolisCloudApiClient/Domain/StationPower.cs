namespace SolisCloudApiClient.Domain;

public record StationPower(DateTime Time, double Pv, double Battery, double Grid, double Load);
