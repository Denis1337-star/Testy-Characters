using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private LocationsConfig _locationsConfig;
    [SerializeField] private HeroesConfig _heroesConfig;

    public override void InstallBindings()
    {
        Container.BindInstance(_locationsConfig);
        Container.BindInstance(_heroesConfig);

        Container.Bind<GameState>().AsSingle();
        Container.Bind<CombatService>().AsSingle();
        Container.Bind<LevelService>().AsSingle();
        Container.Bind<LocationService>().AsSingle();
        Container.Bind<HeroService>().AsSingle();
        Container.Bind<RebirthService>().AsSingle();
    }
}
