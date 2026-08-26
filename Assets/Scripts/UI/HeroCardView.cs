using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroCardView : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] TMP_Text _powerText;
    [SerializeField] Button _upgradeButton;
    [SerializeField] TMP_Text _upgradeLabel;
    [SerializeField] TMP_Text _costText;
    [SerializeField] Button _skillsRowButton;
    [SerializeField] Transform _skillsContainer;
    [SerializeField] HeroSkillIcconView _skillIconPrefab;

    private HeroSkillsPanelView _skillPanel;
    readonly List<HeroSkillIcconView> _skillIconList = new();
    bool _skillsBuilt;

    private int _index;
    HeroService _heroService;
    GameState _state;
    CombatService _combatService;
    private bool _isBound;

    public void Setup(int index, HeroService heroService, GameState gameState,
        CombatService combatService,HeroSkillsPanelView heroSkillsPanelView)
    {
        _index = index;
        _heroService = heroService;
        _state = gameState;
        _combatService = combatService;
        _skillPanel = heroSkillsPanelView;

        if (!_isBound)
        {
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            _skillsRowButton.onClick.AddListener(OnSkillsRowClicked);
            _heroService.Upgraded+= Refresh;
            _heroService.MultiplierChanged += Refresh;
            _combatService.GoldChanged += Refresh;
            _isBound = true;
        }

        var def = _heroService.GetDifinition(index);
        _icon.sprite = def.Icon;
        _nameText.text = def.Name;
        BuildSkillIconOnce();
        Refresh();
    }
    private void OnDestroy()
    {
        _heroService.Upgraded -= Refresh;
        _heroService.MultiplierChanged -= Refresh;
        _combatService.GoldChanged -= Refresh;
        _skillsRowButton.onClick.RemoveListener(OnSkillsRowClicked);
    }
    private void OnUpgradeClicked()
    {
        _heroService.TryUpgrade(_index);
    }
    private void Refresh()
    {
        int lvl = _heroService.GetLevel(_index);
        var def = _heroService.GetDifinition(_index);
        int mult = _heroService.UpgradeMultiplier;

        if (lvl > 0)
        {
            _levelText.text = $"Уровень {lvl}";

            if (mult == 1)
                _upgradeLabel.text = $"Уровень +";
            else
                _upgradeLabel.text = $"x{mult}";
        }
        else
        {
            _levelText.text = "Не куплен";

            if (mult == 1)
                _upgradeLabel.text = $"Купить";
            else
                _upgradeLabel.text = $"x{mult}";
        }

        double cost = _heroService.GetUpgradeCost(_index,mult);
        _costText.text = NumberFormatter.Format(cost);
         
        double power = _heroService.GetPower(_index);
        if (lvl <= 0)
            power = def.BasePower;

        if (def.IsClickHero)
            _powerText.text = $"{NumberFormatter.Format(power)} Урон клика";
        else
            _powerText.text = $"{NumberFormatter.Format(power)} УВС";

        _upgradeButton.interactable = _heroService.CanAffordUpgarade(_index);

        RefreshSkills();
    }
    private void BuildSkillIconOnce()
    {
        if (_skillsBuilt) return;
        _skillsBuilt = true;

        int count = _heroService.GetSkillCount(_index);
        for (int s = 0; s < count; s++)
        {
            var view = Instantiate(_skillIconPrefab, _skillsContainer);
            var skill = _heroService.GetSkill(_index, s);
            view.Bind(skill.Icon);
            _skillIconList.Add(view);
        }
    }
    private void RefreshSkills()
    {
        int level = _heroService.GetLevel(_index);

        if (level <= 0)
        {
            _skillsRowButton.gameObject.SetActive(false);
            return;
        }

        _skillsRowButton.gameObject.SetActive(true);

        for (int i = 0; i < _skillIconList.Count; i++)
        {
            bool isUnlocked  = _heroService.IsSkillVisibleOnCard(_index, i);
            bool isOwnde = _heroService.IsSkillOwned(_index, i);
            bool isCanAfford = _heroService.CanAffordSkill(_index, i);
            _skillIconList[i].RefreshState(isUnlocked, isOwnde, isCanAfford);
        }
    }
    private void OnSkillsRowClicked()
    {
        if (_heroService.GetLevel(_index) <= 0) return;
        _skillPanel.Open(_index);
    }
}
