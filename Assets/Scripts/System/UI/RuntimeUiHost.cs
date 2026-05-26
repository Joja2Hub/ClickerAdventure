using UnityEngine;
using UnityEngine.UI;

public static class RuntimeUiHost
{
    private const string MainCanvasName = "UiCanvas";
    private const string PanelsRootName = "PanelsUI";
    private const string ButtonRootName = "ButtonGroup";
    private const string PopupRootName = "RuntimePopups";
    private const string HudRootName = "RuntimeHud";

    public static Canvas GetCanvas(Transform context = null, int fallbackSortingOrder = 900)
    {
        Canvas contextCanvas = context != null ? context.GetComponentInParent<Canvas>() : null;
        if (contextCanvas != null)
            return contextCanvas;

        Canvas namedCanvas = FindCanvasByName(MainCanvasName);
        if (namedCanvas != null)
            return namedCanvas;

        Canvas firstCanvas = Object.FindFirstObjectByType<Canvas>();
        if (firstCanvas != null)
            return firstCanvas;

        return CreateFallbackCanvas(fallbackSortingOrder);
    }

    public static Transform GetPanelsRoot(Canvas canvas)
    {
        Transform panelsRoot = FindDeepChild(canvas.transform, PanelsRootName);
        return panelsRoot != null ? panelsRoot : canvas.transform;
    }

    public static Transform GetButtonRoot(Canvas canvas)
    {
        Transform buttonRoot = FindDeepChild(canvas.transform, ButtonRootName);
        return buttonRoot != null ? buttonRoot : canvas.transform;
    }

    public static Transform GetPopupRoot(Canvas canvas)
    {
        return GetOrCreateStretchedRoot(canvas.transform, PopupRootName);
    }

    public static Transform GetHudRoot(Canvas canvas)
    {
        return GetOrCreateStretchedRoot(canvas.transform, HudRootName);
    }

    public static bool UsesLayout(Transform parent)
    {
        return parent != null && parent.GetComponent<LayoutGroup>() != null;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Canvas FindCanvasByName(string canvasName)
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.name == canvasName)
                return canvas;
        }

        return null;
    }

    private static Transform GetOrCreateStretchedRoot(Transform parent, string name)
    {
        Transform root = FindDirectChild(parent, name);
        if (root != null)
        {
            root.SetAsLastSibling();
            return root;
        }

        GameObject rootObject = new GameObject(name, typeof(RectTransform));
        rootObject.transform.SetParent(parent, false);
        RectTransform rect = rootObject.GetComponent<RectTransform>();
        Stretch(rect);
        rootObject.transform.SetAsLastSibling();
        return rootObject.transform;
    }

    private static Transform FindDirectChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
        }

        return null;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform found = FindDeepChild(child, name);
            if (found != null)
                return found;
        }

        return null;
    }

    private static Canvas CreateFallbackCanvas(int sortingOrder)
    {
        GameObject canvasObject = new GameObject("RuntimeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }
}
