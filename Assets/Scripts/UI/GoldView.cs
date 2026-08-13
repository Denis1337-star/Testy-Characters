using TMPro;
using UnityEngine;
using Zenject;

public class GoldView : MonoBehaviour
{
    [SerializeField] TMP_Text _goldText;
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
        _goldText.text = NumberFormatter.Format(_state.gold);
    }

}
