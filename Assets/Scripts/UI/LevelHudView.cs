using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LevelHudView : MonoBehaviour
{
    [SerializeField] private TMP_Text _lastLevelText;
    [SerializeField] private TMP_Text _currentLevelText;
    [SerializeField] private TMP_Text _nextLevelText;

    [SerializeField] private Image _progressWave;
    [SerializeField] private TMP_Text _killsText;

    [SerializeField] private Button _lastLevelButton;
    [SerializeField] private Button _nextLevelButton;

    GameState _state;

    [Inject]
    private void Construct(GameState gameState)
    {
        _state = gameState;
    }
    private void Start()
    {
        _state.Changed += Refresh;
        Refresh();
    }
    private void OnDestroy()
    {
        _state.Changed -= Refresh;
    }
    private void Refresh()
    {
        _lastLevelText.text = $"{_state.currentLevel - 1}";
        _currentLevelText.text = $"{_state.currentLevel}";
        _nextLevelText.text = $"{_state.currentLevel + 1}";

        _progressWave.fillAmount = (float)_state.killsOnLevel / _state.killsToClear;
        _killsText.text = $"{_state.killsOnLevel}/{_state.killsToClear}";
    }
}
