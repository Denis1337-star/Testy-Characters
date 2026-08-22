using TMPro;
using UnityEngine;
using Zenject;

public class BossTimerView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _timeText;
    GameState _state;

    [Inject]
    private void Construct(GameState gameState)
    {
        _state = gameState;
    }
    private void Update()
    {
        bool show = _state.isBossActive;

        if (_root.activeSelf != show)
            _root.SetActive(show);

        if (!show) return;

        _timeText.text = $"{_state.bossTimerLeft:0.0} сек";
    }
}
