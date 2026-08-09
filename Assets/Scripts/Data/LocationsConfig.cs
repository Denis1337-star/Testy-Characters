using UnityEngine;

[CreateAssetMenu(menuName = "Game/Locations Config")]
public class LocationsConfig : ScriptableObject
{
    public int LevelsPerLocation = 50;
    public LocationData[] Locations;
}
