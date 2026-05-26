using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TownZoneHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Image image;
    private Color normalColor = Color.white;
    private Vector3 normalScale = Vector3.one;
    private bool isPointerInside;

    public void Initialize()
    {
        image = GetComponent<Image>();
        normalScale = transform.localScale;

        if (image != null)
            normalColor = image.color;
    }

    private void OnDisable()
    {
        ResetVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        ApplyVisual(new Color(1f, 0.92f, 0.48f, 1f), 1.06f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        ResetVisual();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ApplyVisual(new Color(0.55f, 0.9f, 1f, 1f), 0.98f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPointerInside)
            ApplyVisual(new Color(1f, 0.92f, 0.48f, 1f), 1.06f);
        else
            ResetVisual();
    }

    private void ApplyVisual(Color color, float scale)
    {
        if (image != null)
            image.color = color;

        transform.localScale = normalScale * scale;
    }

    private void ResetVisual()
    {
        if (image != null)
            image.color = normalColor;

        transform.localScale = normalScale;
    }
}
