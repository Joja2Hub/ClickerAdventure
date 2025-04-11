using UnityEngine;

public class MapController : MonoBehaviour
{
    public static MapController Instance; // ������ �� ������������ ���������
    public Camera mainCamera => FindAnyObjectByType<Camera>(); // ������� ������
    public SpriteRenderer mapSprite; // ������ �����

    public float moveSpeed = 5f; // �������� �����������
    public float zoomSpeed = 5f; // �������� ����
    public float minZoom = 2f; // ����������� ��� (������������ �����������)
    private float maxZoom; // ������������ ��� (������������ ���������)

    private Vector2 mapBoundsMin; // ����������� ������� �����
    private Vector2 mapBoundsMax; // ������������ ������� �����

    public bool isMovementEnabled = true;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� ����� ��������� ������ ����� �������
        }
        else
        {
            Destroy(gameObject); // ���������� ������ ����������
        }
    }

    private void Start()
    {
        if (mapSprite == null)
        {
            Debug.LogError("Map sprite is not assigned!");
            return;
        }

        // ������������ ������� �����
        CalculateMapBounds();

        // ������������ ������������ ���
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

        // ��������� ��������� ���� ��� ������������
        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * moveSpeed * Time.deltaTime;
            mainCamera.transform.position -= new Vector3(mouseDelta.x, mouseDelta.y, 0);
        }
    }

    private void HandleZoom()
    {
        float zoomInput = 0f;

        // ��������� �������� ����
        zoomInput += Input.GetAxis("Mouse ScrollWheel");

        // ��������� ����-����� (��� ��������� ���������)
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            float previousDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);

            zoomInput += (currentDistance - previousDistance) * Time.deltaTime;
        }

        // ��������� ���
        float newSize = mainCamera.orthographicSize - zoomInput * zoomSpeed;
        mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);

        // ������������� ������� ����� ����� ��������� ����
        ClampCameraPosition();
    }

    private void ClampCameraPosition()
    {
        // �������� ������� ������� ������� ������
        float screenAspect = (float)Screen.width / Screen.height;
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * screenAspect;

        // ������������ ������� ������
        Vector3 newPosition = mainCamera.transform.position;
        newPosition.x = Mathf.Clamp(newPosition.x, mapBoundsMin.x + cameraWidth / 2, mapBoundsMax.x - cameraWidth / 2);
        newPosition.y = Mathf.Clamp(newPosition.y, mapBoundsMin.y + cameraHeight / 2, mapBoundsMax.y - cameraHeight / 2);
        mainCamera.transform.position = newPosition;
    }

    private void CalculateMapBounds()
    {
        // �������� ������� ������� �����
        float spriteWidth = mapSprite.bounds.size.x;
        float spriteHeight = mapSprite.bounds.size.y;

        // �������� ������� �������
        Vector3 spritePosition = mapSprite.transform.position;

        // ������������ ������� �����
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
        // �������� ������� ������
        float screenAspect = (float)Screen.width / Screen.height;

        // �������� ������� �����
        float mapWidth = mapSprite.bounds.size.x;
        float mapHeight = mapSprite.bounds.size.y;

        // ������������ ������������ orthographicSize
        float maxOrthoHeight = mapHeight / 2f;
        float maxOrthoWidth = mapWidth / (2f * screenAspect);

        // �������� ����������� ��������, ����� ����� ���������� �� ������
        maxZoom = Mathf.Min(maxOrthoHeight, maxOrthoWidth);
    }
}