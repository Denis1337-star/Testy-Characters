using TMPro;
using UnityEngine;
using Zenject;

public class BossTimerView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _timeText;
    GameState _gameState;

    [Inject]
    private void Construct(GameState gameState)
    {
        _gameState = gameState;
    }
    private void Update()
    {
        
        bool show = _gameState.isBossActive;

        if (_root.activeSelf != show)
            _root.SetActive(show);

        if (!show) return;

        _timeText.text = $"{_gameState.bossTimerLeft:0.0} сек";
    }
}
