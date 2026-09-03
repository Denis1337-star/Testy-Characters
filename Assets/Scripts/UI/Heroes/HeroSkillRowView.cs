using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSkillRowView : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _descriptionText;
    [SerializeField] Button _buyButton;
    [SerializeField] TMP_Text _costText;
    [SerializeField] GameObject _checkmark;
    private int _heroIndex;
    private int _skillIndex;
    HeroService _heroService;

    public void Setup(int heroIndex, int skillIndex, HeroService heroService)
    {
        _heroIndex = heroIndex;
        _skillIndex = skillIndex;
        _heroService = heroService;

        var skill = _heroService.GetSkill(_heroIndex, _skillIndex);
        _icon.sprite = skill.Icon;
        _nameText.text = skill.Name;
        _descriptionText.text = skill.Description;

        _buyButton.onClick.RemoveAllListeners();
        _buyButton.onClick.AddListener(OnBuy);

        Refresh();
    }
    public void Refresh()
    {
        bool isOwned = _heroService.IsSkillOwned(_heroIndex, _skillIndex);
        bool canBuy = _heroService.CanAffordSkill(_heroIndex, _skillIndex);
        var skill = _heroService.GetSkill(_heroIndex, _skillIndex);

        if (isOwned)
        {
            _buyButton.gameObject.SetActive(false);
            _checkmark.gameObject.SetActive(true);
        }
        else
        {
            _buyButton.gameObject.SetActive(true);
            _checkmark.gameObject.SetActive(false);
            _costText.text = $"Стоимость: \n {NumberFormatter.Format(skill.Cost)}";
            _buyButton.interactable = canBuy;
        }
    }
    private void OnBuy()
    {
        _heroService.TryBuySkill(_heroIndex, _skillIndex);
    }
}
