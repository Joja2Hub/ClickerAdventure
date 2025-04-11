using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void Update()
    {
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
            Debug.Log($"Attempt to interact with point: {pointName}");

            if (PlayerStats.Instance.level >= requiredLevel)
            {
                Debug.Log($"Access granted to point: {pointName}");

                switch (pointType)
                {
                    case PointType.Dungeon:
                        Debug.Log($"Opening battle screen for dungeon: {pointName}");
                        StartBattle();
                        break;

                    case PointType.Town:
                        Debug.Log($"Loading town scene for point: {pointName}");
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
        // Здесь можно загрузить сцену города
        Debug.Log("Loading town scene...");
        //SceneManager.LoadScene("TownScene");
    }

    private void StartBattle()
    {
        DungeonTransferData.LocationData = locationData;
        SceneManager.LoadScene("BattleScene");
    }

}