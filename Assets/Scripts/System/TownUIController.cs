using UnityEngine;
using UnityEngine.UI;

public class TownUIController : MonoBehaviour
{
    public Image backgroundImage;
    public RectTransform shopButton;
    public RectTransform guildButton;
    public RectTransform innButton;

    public void Setup(TownData data)
    {
        if (data == null)
        {
            Debug.LogWarning("TownUIController.Setup called without TownData.");
            return;
        }

        if (backgroundImage != null)
            backgroundImage.sprite = data.background;
        else
            Debug.LogWarning("TownUIController has no background image assigned.");

        if (shopButton != null)
            shopButton.anchoredPosition = data.shopPosition;

        if (guildButton != null)
            guildButton.anchoredPosition = data.guildPosition;

        if (innButton != null)
            innButton.anchoredPosition = data.innPosition;

        ConfigureZoneHighlight(shopButton);
        ConfigureZoneHighlight(guildButton);
        ConfigureZoneHighlight(innButton);
    }

    public void hideWindow()
    {
        this.gameObject.SetActive(false);
    }

    private void ConfigureZoneHighlight(RectTransform zone)
    {
        if (zone == null)
            return;

        Image image = zone.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = true;

        Button button = zone.GetComponent<Button>();
        if (button != null && image != null)
            button.targetGraphic = image;

        TownZoneHighlight highlight = zone.GetComponent<TownZoneHighlight>();
        if (highlight == null)
            highlight = zone.gameObject.AddComponent<TownZoneHighlight>();

        highlight.Initialize();
    }
}
