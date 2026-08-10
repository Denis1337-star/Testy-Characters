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
    [SerializeField] private Image _nextLockIcon;

    GameState _state;
    LevelService _levelService;

    [Inject]
    private void Construct(GameState gameState, LevelService levelService)
    {
        _state = gameState;
        _levelService = levelService;
    }
    private void Start()
    {
        _lastLevelButton.onClick.AddListener(OnLastLevelClicked);
        _nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        _state.Changed += Refresh;
        Refresh();
    }
    private void OnDestroy()
    {
        if (_state != null)
            _state.Changed -= Refresh;

        if (_lastLevelButton != null)
            _lastLevelButton.onClick.RemoveListener(OnLastLevelClicked);

        if (_nextLevelButton != null)
            _nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
    }
    private void OnLastLevelClicked()
    {
        _levelService.TrySelectLevel(_state.currentLevel - 1);
        Refresh();
    }
    private void OnNextLevelClicked()
    {
        _levelService.TrySelectLevel(_state.currentLevel + 1);
        Refresh();
    }
    private void Refresh()
    {
        _lastLevelText.text = $"{_state.currentLevel - 1}";
        _currentLevelText.text = $"{_state.currentLevel}";
        _nextLevelText.text = $"{_state.currentLevel + 1}";

        _killsText.text = $"{_state.killsOnLevel}/{_state.killsToClear}";
        _progressWave.fillAmount = (float)_state.killsOnLevel / _state.killsToClear;

        _lastLevelButton.gameObject.SetActive(_state.currentLevel > 1);

        bool canGoNext = _state.currentLevel < _state.maxUnlockedLevel;
        _nextLevelButton.interactable = canGoNext;
        _nextLockIcon.gameObject.SetActive(!canGoNext);
    }
}
