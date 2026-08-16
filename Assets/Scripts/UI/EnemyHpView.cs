using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EnemyHpView : MonoBehaviour
{
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private Image _hpFill;

    private GameState _state;

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
        _hpText.text = $"HP {NumberFormatter.Format(_state.enemyhp)}";
        _hpFill.fillAmount = (float)(_state.enemyhp / _state.enemyMaxHp);
    }
}
