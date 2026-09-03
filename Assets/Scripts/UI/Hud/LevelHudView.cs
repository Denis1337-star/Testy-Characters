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

    GameState _gameState;
    LevelService _levelService;

    [Inject]
    private void Construct(GameState gameState, LevelService levelService)
    {
        _gameState = gameState;
        _levelService = levelService;
    }
    private void Start()
    {
        _lastLevelButton.onClick.AddListener(OnLastLevelClicked);
        _nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        _levelService.ZoneChanged += Refresh;
        _levelService.ProgressChanged += Refresh;
        Refresh();
    }
    private void OnDestroy()
    {
        _levelService.ZoneChanged -= Refresh;
        _levelService.ProgressChanged -= Refresh;

        _lastLevelButton.onClick.RemoveListener(OnLastLevelClicked);
        _nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
    }
    private void OnLastLevelClicked()
    {
        _levelService.TrySelectLevel(_gameState.currentLevel - 1);
        Refresh();
    }
    private void OnNextLevelClicked()
    {
        _levelService.TrySelectLevel(_gameState.currentLevel + 1);
        Refresh();
    }
    private void Refresh()
    {
        _lastLevelText.text = $"{_gameState.currentLevel - 1}";
        _currentLevelText.text = $"{_gameState.currentLevel}";
        _nextLevelText.text = $"{_gameState.currentLevel + 1}";

        _killsText.text = $"{_gameState.killsOnLevel}/{_gameState.killsToClear}";
        _progressWave.fillAmount = (float)_gameState.killsOnLevel / _gameState.killsToClear;

        _lastLevelButton.gameObject.SetActive(_gameState.currentLevel > 1);

        bool canGoNext = _gameState.currentLevel < _gameState.maxUnlockedLevel;
        _nextLevelButton.interactable = canGoNext;
        _nextLockIcon.gameObject.SetActive(!canGoNext);
    }
}
