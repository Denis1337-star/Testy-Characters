using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EnemyHpView : MonoBehaviour
{
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private Image _hpFill;

    private GameState _state;
    private CombatService _combatService;

    [Inject]
    private void Construct(GameState gameState, CombatService combatService)
    {
        _state = gameState;
        _combatService = combatService;
    }
    private void Start()
    {
       _combatService.HpChanged += Refresh;
        Refresh();
    }
    private void OnDestroy()
    {
        _combatService.HpChanged-= Refresh;
    }
    private void Refresh()
    {
        _hpText.text = $"HP {NumberFormatter.Format(_state.enemyhp)}";
        _hpFill.fillAmount = (float)(_state.enemyhp / _state.enemyMaxHp);
    }
}
