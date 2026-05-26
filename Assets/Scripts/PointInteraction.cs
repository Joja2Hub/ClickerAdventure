using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum PointType
{
    Dungeon,
    Town
}

public class PointInteraction : MonoBehaviour
{
    public string pointName;
    public int requiredLevel;
    public PointType pointType;
    public LocationData locationData;
    public TownData townData;
    public GameObject townUIPrefab;

    private GameObject lockOverlayRoot;
    private SpriteRenderer lockOverlay;
    private TextMeshPro lockText;
    private static Sprite lockOverlaySprite;

    private void Start()
    {
        SubscribeToLevelChanges();
        RefreshLockOverlay();
    }

    private void OnEnable()
    {
        RefreshLockOverlay();
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnLevelChanged -= OnPlayerLevelChanged;
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                HandleTouch(touch.position);
        }

        if (Input.GetMouseButtonDown(0))
            HandleTouch(Input.mousePosition);
    }

    private void HandleTouch(Vector3 touchPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2 touchWorldPosition = new Vector2(worldPosition.x, worldPosition.y);

        Collider2D hit = Physics2D.OverlapPoint(touchWorldPosition);
        if (hit == null || !hit.transform.IsChildOf(transform))
            return;

        if (IsLocked())
        {
            ShowLockedFeedback();
            return;
        }

        switch (pointType)
        {
            case PointType.Dungeon:
                StartBattle();
                break;
            case PointType.Town:
                LoadTownWindow();
                break;
        }
    }

    private void LoadTownWindow()
    {
        if (townUIPrefab == null)
            return;

        townUIPrefab.SetActive(true);
        var townUI = townUIPrefab.GetComponent<TownUIController>();
        if (townUI == null)
            townUI = townUIPrefab.GetComponentInChildren<TownUIController>(true);

        if (townUI != null && townData != null)
            townUI.Setup(townData);
        else if (townUI == null)
            Debug.LogWarning($"Town UI prefab for {pointName} has no TownUIController.");
        else
            Debug.LogWarning($"Town point {pointName} has no TownData assigned.");
    }

    private void StartBattle()
    {
        DungeonTransferData.LocationData = locationData;
        SceneManager.LoadScene("BattleScene");
    }

    private void SubscribeToLevelChanges()
    {
        if (PlayerStats.Instance == null)
            return;

        PlayerStats.Instance.OnLevelChanged -= OnPlayerLevelChanged;
        PlayerStats.Instance.OnLevelChanged += OnPlayerLevelChanged;
    }

    private void OnPlayerLevelChanged(int level)
    {
        RefreshLockOverlay();
    }

    private bool IsLocked()
    {
        return PlayerStats.Instance != null && PlayerStats.Instance.level < requiredLevel;
    }

    private void RefreshLockOverlay()
    {
        if (requiredLevel <= 1 || !IsLocked())
        {
            SetLockOverlayVisible(false);
            return;
        }

        EnsureLockOverlay();
        SetLockOverlayVisible(true);

        if (lockText != null)
            lockText.text = $"LOCK\nLv {requiredLevel}";
    }

    private void ShowLockedFeedback()
    {
        int currentLevel = PlayerStats.Instance != null ? PlayerStats.Instance.level : 1;
        string title = string.IsNullOrWhiteSpace(pointName) ? "Location locked" : $"{pointName} locked";
        RewardPopup.ShowMessage(title, $"Required level: {requiredLevel}\nYour level: {currentLevel}");
    }

    private void SetLockOverlayVisible(bool visible)
    {
        if (lockOverlayRoot != null)
            lockOverlayRoot.SetActive(visible);
    }

    private void EnsureLockOverlay()
    {
        if (lockOverlayRoot != null)
            return;

        Bounds bounds = GetPointBounds();
        lockOverlayRoot = new GameObject("LevelLockOverlay");
        lockOverlayRoot.transform.SetParent(transform, false);
        lockOverlayRoot.transform.position = bounds.center;

        GameObject shade = new GameObject("Shade");
        shade.transform.SetParent(lockOverlayRoot.transform, false);
        lockOverlay = shade.AddComponent<SpriteRenderer>();
        lockOverlay.sprite = GetLockOverlaySprite();
        lockOverlay.color = new Color(0f, 0f, 0f, 0.55f);
        lockOverlay.sortingOrder = GetTopSortingOrder() + 30;
        shade.transform.localScale = new Vector3(Mathf.Max(0.5f, bounds.size.x), Mathf.Max(0.5f, bounds.size.y), 1f);

        GameObject labelObject = new GameObject("LockLabel");
        labelObject.transform.SetParent(lockOverlayRoot.transform, false);
        labelObject.transform.localPosition = Vector3.zero;
        lockText = labelObject.AddComponent<TextMeshPro>();
        lockText.alignment = TextAlignmentOptions.Center;
        lockText.fontSize = 4f;
        lockText.fontStyle = FontStyles.Bold;
        lockText.color = Color.white;
        lockText.text = $"LOCK\nLv {requiredLevel}";
        lockText.enableAutoSizing = true;
        lockText.fontSizeMin = 1.6f;
        lockText.fontSizeMax = 4f;
        lockText.sortingOrder = lockOverlay.sortingOrder + 1;

        RectTransform textRect = lockText.GetComponent<RectTransform>();
        if (textRect != null)
            textRect.sizeDelta = new Vector2(Mathf.Max(1.5f, bounds.size.x), Mathf.Max(1f, bounds.size.y * 0.7f));
    }

    private Bounds GetPointBounds()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].bounds);

            return bounds;
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }

        return new Bounds(transform.position, Vector3.one);
    }

    private int GetTopSortingOrder()
    {
        int sortingOrder = 0;
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
            sortingOrder = Mathf.Max(sortingOrder, renderer.sortingOrder);

        return sortingOrder;
    }

    private static Sprite GetLockOverlaySprite()
    {
        if (lockOverlaySprite != null)
            return lockOverlaySprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        lockOverlaySprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return lockOverlaySprite;
    }
}
