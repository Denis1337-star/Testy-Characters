using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CrystalTempleView : MonoBehaviour
{
    [Header("Профиль")]
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private Image _xpFill;
    [SerializeField] private TMP_Text _xpText;

    [Header("Храм")]
    [SerializeField] private TMP_Text _pendingText;
    [SerializeField] private TMP_Text _statsText;

    [SerializeField] private Button _rebirthButton;
    private RebirthService _rebirthService;
    private CombatService _combatService;

    [Inject]
    private void Construct(RebirthService rebirthService, CombatService combatService)
    {
        _rebirthService = rebirthService;
        _combatService = combatService;
    }
    private void Start()
    {
        _rebirthButton.onClick.AddListener(OnRebirthClicked);
        _combatService.GoldChanged += Refresh;
        _rebirthService.Rebirthed += Refresh;
        Refresh();
    }
    private void OnDestroy()
    {
        _rebirthButton.onClick.RemoveListener(OnRebirthClicked);
            _combatService.GoldChanged -= Refresh;
            _rebirthService.Rebirthed -= Refresh;
    }
    public void Open()
    {
        Refresh();
    }

    private void OnRebirthClicked()
    {
        _rebirthService.DoRebirth();
    }
    private void Refresh()
    {
        RebirthPreview preview = _rebirthService.GetPreview(); 
        _levelText.text = $"Уровень: {preview.ProfileLevel}";
        _xpText.text = $"{NumberFormatter.Format(preview.XpIntoLevel)}/{NumberFormatter.Format(preview.XpToNextLevel)} опыта";

        if (preview.XpToNextLevel > 0)
            _xpFill.fillAmount = (float)(preview.XpIntoLevel / preview.XpToNextLevel);
        else
            _xpFill.fillAmount = 0f;

        _pendingText.text =
            $"Камни: {NumberFormatter.Format(preview.CrystalsForExperience)}  заберёшь {NumberFormatter.Format(preview.Pending)}";
        _statsText.text =
            $"Станет кристаллов: {NumberFormatter.Format(preview.CrystalsAfter)}\n" +
            $"Прирост бонуса золота: +{NumberFormatter.Format(preview.GoldBonusDeltaPercent)}%";

        _rebirthButton.interactable = preview.CanRebirth;
    }
}
