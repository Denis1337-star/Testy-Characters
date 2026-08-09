using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ClickInputView : MonoBehaviour
{
    [SerializeField] private Button _button;

    private CombatService _combatService;

    [Inject]
    private void Construct(CombatService combatService)
    {
        _combatService = combatService;
    }
    private void Start()
    {
        _button.onClick.AddListener(_combatService.Click);
    }
}
