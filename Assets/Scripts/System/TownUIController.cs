using UnityEngine;
using UnityEngine.UIElements;

public class TownUIController : MonoBehaviour
{
    public Image backgroundImage;
    public RectTransform shopButton;
    public RectTransform guildButton;
    public RectTransform innButton;

    public void Setup(TownData data)
    {
        backgroundImage.sprite = data.background;
        shopButton.anchoredPosition = data.shopPosition;
        guildButton.anchoredPosition = data.guildPosition;
        innButton.anchoredPosition = data.innPosition;
        // инициализируй остальное
    }

    public void hideWindow()
    {
        this.gameObject.SetActive(false);
    }



}
