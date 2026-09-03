using UnityEngine;
using UnityEngine.UI;

public class HeroSkillIconView : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _checkmark;

    public void Bind(Sprite sprite)
    {
        _icon.sprite = sprite;
        _checkmark.SetActive(false);
        gameObject.SetActive(false);
    }
    public void RefreshState(bool isVisible, bool isOwned, bool canAfford)
    {
        gameObject.SetActive(isVisible);
        if (!isVisible) return;

        _checkmark.SetActive(isOwned);

        if (isOwned || canAfford)
            _icon.color = Color.white;
        else
            _icon.color = Color.gray3;
    }
 
}
