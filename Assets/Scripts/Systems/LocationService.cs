using UnityEngine;

public class LocationService 
{
    readonly GameState _gameState;
    readonly LocationsConfig _locationConfig;

    public LocationService(GameState state, LocationsConfig config)
    {
        _gameState = state;
        _locationConfig = config;
    }

    public int GetLocationIndex()
    {
        if (_locationConfig.Locations == null || _locationConfig.Locations.Length == 0)
            return 0;

        int perLocation = Mathf.Max(1, _locationConfig.LevelsPerLocation);
        int index = (_gameState.currentLevel - 1) / perLocation;
        return index % _locationConfig.Locations.Length;
    }

    public LocationData GetCurrentLocation()
    {
        int i = GetLocationIndex();
        return _locationConfig.Locations[i];
    }

    public RuntimeAnimatorController[] GetEnemyPool()
    {
        var location = GetCurrentLocation();

        if (location == null || location.Enemies == null)
            return System.Array.Empty<RuntimeAnimatorController>();
        return location.Enemies;
    }
    public Sprite GetBackground()
    {
        var location = GetCurrentLocation();
        return location != null ? location.Background : null;
    }
}
