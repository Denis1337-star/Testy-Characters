using UnityEngine;

public class LocationService 
{
    readonly GameState _state;
    readonly LocationsConfig _config;

    public LocationService(GameState state, LocationsConfig config)
    {
        _state = state;
        _config = config;
    }

    public int GetLocationIndex()
    {
        if (_config.Locations == null || _config.Locations.Length == 0)
            return 0;

        int per = Mathf.Max(1, _config.LevelsPerLocation);
        int index = (_state.currentLevel - 1) / per;
        return index % _config.Locations.Length;
    }

    public LocationData GetCurrentLocation()
    {
        int i = GetLocationIndex();
        return _config.Locations[i];
    }

    public RuntimeAnimatorController[] GetEnemyPool()
    {
        var loc = GetCurrentLocation();

        if (loc == null || loc.Enemies == null)
            return System.Array.Empty<RuntimeAnimatorController>();
        return loc.Enemies;
    }
    public Sprite GetBackground()
    {
        var loc = GetCurrentLocation();
        return loc != null ? loc.Background : null;
    }
}
