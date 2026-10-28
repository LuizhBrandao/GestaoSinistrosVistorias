using Sinistros.Domain.Exceptions;

namespace Sinistros.Domain.ValueObjects;

public readonly record struct CoordenadasGps
{
    public double Latitude { get; }
    public double Longitude { get; }

    public CoordenadasGps(double latitude, double longitude)
    {
        if (latitude is < -90.0 or > 90.0)
            throw new RegraNegocioException("LATITUDE_INVALIDA", $"Latitude inválida: {latitude}. Deve estar entre -90 e 90 graus.");

        if (longitude is < -180.0 or > 180.0)
            throw new RegraNegocioException("LONGITUDE_INVALIDA", $"Longitude inválida: {longitude}. Deve estar entre -180 e 180 graus.");

        Latitude = latitude;
        Longitude = longitude;
    }

    public override string ToString() => $"{Latitude:F6}, {Longitude:F6}";
}
