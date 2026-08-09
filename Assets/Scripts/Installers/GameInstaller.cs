using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private LocationsConfig _locationsConfig;

    public override void InstallBindings()
    {
        Container.BindInstance(_locationsConfig);

        Container.Bind<GameState>().AsSingle();
        Container.Bind<CombatService>().AsSingle();
        Container.Bind<LevelService>().AsSingle();
        Container.Bind<LocationService>().AsSingle();

  }
}
