using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class HeroSkillsPanelView : MonoBehaviour
{
    [SerializeField] GameObject _root;
    [SerializeField] Button _closeButton;
    [SerializeField] Button _dimmerButton;
    [SerializeField] Image _heroIcon;
    [SerializeField] TMP_Text _nameHeroText;
    [SerializeField] TMP_Text _levelHeroText;
    [SerializeField] TMP_Text _powerText;
    [SerializeField] Transform _content;
    [SerializeField] HeroSkillRowView _rowPrefab;

    private HeroService _heroService;
    private int _heroIndex = 1;
    readonly List<HeroSkillRowView> _rows = new();

    [Inject]
    private void Construct(HeroService heroService)
    {
        _heroService = heroService;
    }
    private void Start()
    {
        _closeButton.onClick.AddListener(Close);
        _dimmerButton.onClick.AddListener(Close);

        _heroService.Upgraded += RefreshIfOpen;
        _root.SetActive(false);
    }
    private void OnDestroy()
    {
        _heroService.Upgraded -= RefreshIfOpen;
    }
    public void Open(int heroIndex)
    {
        _heroIndex = heroIndex;
        _root.SetActive(true);
        Rebuild();
    }
    public void Close()
    {
        _root.SetActive(false);
        _heroIndex = -1;
    }
    private void RefreshIfOpen()
    {
        if (_heroIndex < 0) return;
        Rebuild();
    }
    private void Rebuild()
    {
        var def = _heroService.GetDefinition(_heroIndex);
        _heroIcon.sprite = def.Icon;
        _nameHeroText.text = def.Name;
        _levelHeroText.text = $"Уровень: {_heroService.GetLevel(_heroIndex)}";

        double power = _heroService.GetPower(_heroIndex);
        if (def.IsClickHero == true)
            _powerText.text = $"Урон клика: {NumberFormatter.Format(power)}";
        else
            _powerText.text = $"УВС (урон в секунду): {NumberFormatter.Format(power)}";

        for(int i =_rows.Count-1; i>=0; i--)
        {
            Destroy(_rows[i].gameObject);
        }
        _rows.Clear();

        int count = _heroService.GetSkillCount(_heroIndex);
        for (int s = 0; s < count; s++)
        {
            var row = Instantiate(_rowPrefab, _content);
            row.Setup(_heroIndex, s, _heroService);
            _rows.Add(row);
        }
    }

}
