using UnityEngine;

public class MapController : MonoBehaviour
{
    public static MapController Instance; // Ссылка на единственный экземпляр
    public Camera mainCamera => FindAnyObjectByType<Camera>(); // Главная камера
    public SpriteRenderer mapSprite; // Спрайт карты

    public float moveSpeed = 5f; // Скорость перемещения
    public float zoomSpeed = 5f; // Скорость зума
    public float minZoom = 2f; // Минимальный зум (максимальное приближение)
    private float maxZoom; // Максимальный зум (максимальное отдаление)

    private Vector2 mapBoundsMin; // Минимальные границы карты
    private Vector2 mapBoundsMax; // Максимальные границы карты

    public bool isMovementEnabled = true;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Если нужно сохранить объект между сценами
        }
        else
        {
            Destroy(gameObject); // Уничтожаем лишние экземпляры
        }
    }

    private void Start()
    {
        if (mapSprite == null)
        {
            Debug.LogError("Map sprite is not assigned!");
            return;
        }

        // Рассчитываем границы карты
        CalculateMapBounds();

        // Рассчитываем максимальный зум
        CalculateMaxZoom();
    }

    public void EnableMovement(bool enable)
    {
        isMovementEnabled = enable;
    }

    private void Update()
    {
        if (!isMovementEnabled) return;
        HandleMovement();
        HandleZoom();
        ClampCameraPosition();
    }

    private void HandleMovement()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 touchDelta = touch.deltaPosition * moveSpeed * Time.deltaTime;
                mainCamera.transform.position -= new Vector3(touchDelta.x, touchDelta.y, 0);
            }
        }

        // Добавляем поддержку мыши для тестирования
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * moveSpeed * Time.deltaTime;
            mainCamera.transform.position -= new Vector3(mouseDelta.x, mouseDelta.y, 0);
        }
    }

    private void HandleZoom()
    {
        float zoomInput = 0f;

        // Поддержка колесика мыши
        zoomInput += Input.GetAxis("Mouse ScrollWheel");

        // Поддержка пинч-жеста (для мобильных устройств)
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            float previousDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);

            zoomInput += (currentDistance - previousDistance) * Time.deltaTime;
        }

        // Применяем зум
        float newSize = mainCamera.orthographicSize - zoomInput * zoomSpeed;
        mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);

        // Пересчитываем границы карты после изменения зума
        ClampCameraPosition();
    }

    private void ClampCameraPosition()
    {
        // Получаем размеры видимой области камеры
        float screenAspect = (float)Screen.width / Screen.height;
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * screenAspect;

        // Ограничиваем позицию камеры
        Vector3 newPosition = mainCamera.transform.position;
        newPosition.x = Mathf.Clamp(newPosition.x, mapBoundsMin.x + cameraWidth / 2, mapBoundsMax.x - cameraWidth / 2);
        newPosition.y = Mathf.Clamp(newPosition.y, mapBoundsMin.y + cameraHeight / 2, mapBoundsMax.y - cameraHeight / 2);
        mainCamera.transform.position = newPosition;
    }

    private void CalculateMapBounds()
    {
        // Получаем размеры спрайта карты
        float spriteWidth = mapSprite.bounds.size.x;
        float spriteHeight = mapSprite.bounds.size.y;

        // Получаем позицию спрайта
        Vector3 spritePosition = mapSprite.transform.position;

        // Рассчитываем границы карты
        mapBoundsMin = new Vector2(
            spritePosition.x - spriteWidth / 2,
            spritePosition.y - spriteHeight / 2
        );

        mapBoundsMax = new Vector2(
            spritePosition.x + spriteWidth / 2,
            spritePosition.y + spriteHeight / 2
        );
    }

    private void CalculateMaxZoom()
    {
        // Получаем размеры экрана
        float screenAspect = (float)Screen.width / Screen.height;

        // Получаем размеры карты
        float mapWidth = mapSprite.bounds.size.x;
        float mapHeight = mapSprite.bounds.size.y;

        // Рассчитываем максимальный orthographicSize
        float maxOrthoHeight = mapHeight / 2f;
        float maxOrthoWidth = mapWidth / (2f * screenAspect);

        // Выбираем минимальное значение, чтобы карта помещалась на экране
        maxZoom = Mathf.Min(maxOrthoHeight, maxOrthoWidth);
    }
}