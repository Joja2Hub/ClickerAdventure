using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public enum PointType
{
    Dungeon, // Данж
    Town     // Населённый пункт
}

public class PointInteraction : MonoBehaviour
{
    public string pointName; // Название точки
    public int requiredLevel; // Требуемый уровень для входа
    public PointType pointType; // Тип точки
    public LocationData locationData; // Данные локации (для данжа)
    public TownData townData;

    public GameObject townUIPrefab;


    private void Awake()
    {
      

    }

    private void Update()
    {
        // Блокируем клик, если он был по UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // Обработка касаний
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HandleTouch(touch.position);
            }
        }

        // Обработка кликов мыши (для тестирования в редакторе)
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition);
        }
    }


    private void HandleTouch(Vector3 touchPosition)
    {
        // Преобразуем позицию касания в мировые координаты
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2 touchWorldPosition = new Vector2(worldPosition.x, worldPosition.y);

        // Проверяем, попал ли касание в коллайдер
        Collider2D hit = Physics2D.OverlapPoint(touchWorldPosition);
        if (hit != null && hit.transform.IsChildOf(transform))
        {

            if (PlayerStats.Instance.level >= requiredLevel)
            {
                switch (pointType)
                {
                    case PointType.Dungeon:
                        StartBattle();
                        break;

                    case PointType.Town:
                        LoadTownScene();
                        break;
                }
            }
            else
            {
                Debug.Log($"Access denied to point: {pointName}. Required level: {requiredLevel}");
            }
        }
    }

    private void LoadTownScene()
    {
        Debug.Log("Открытие");
        townUIPrefab.gameObject.SetActive(true);
    }



    private void StartBattle()
    {
        DungeonTransferData.LocationData = locationData;
        SceneManager.LoadScene("BattleScene");
    }

}