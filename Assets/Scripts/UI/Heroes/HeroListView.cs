using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class HeroListView : MonoBehaviour
{
    [SerializeField] HeroCardView _cardPrefab;
    [SerializeField] Transform _content;
    [SerializeField] HeroSkillsPanelView _skillsPanel;

    HeroService _heroService;
    GameState _gameState;
    CombatService _combatService;
    readonly List<HeroCardView> _spawned = new();

    [Inject]
    private void Construct(GameState gameState, HeroService heroService, CombatService combatService)
    {
        _gameState = gameState;
        _heroService = heroService;
        _combatService = combatService;
    }

    private void Start()
    {
        _heroService.ListChanged += Rebuild;
        Rebuild();
    }
    private void OnDestroy()
    {
        _heroService.ListChanged -= Rebuild;
    }
    private void Rebuild()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            Destroy(_spawned[i].gameObject);
        }
        _spawned.Clear();

        for (int i = 0; i < _heroService.HeroCount; i++)
        {
            if (!_heroService.IsVisible(i)) continue;

            var card = Instantiate(_cardPrefab, _content);
            card.Setup(i, _heroService, _combatService, _skillsPanel, _gameState);
            _spawned.Add(card);
            card.Setup(i, _heroService, _combatService, _skillsPanel, _gameState);
        }
    }
}
