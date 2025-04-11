using UnityEngine;
using UnityEngine.SceneManagement;

public enum PointType
{
    Dungeon, // ����
    Town     // ��������� �����
}

public class PointInteraction : MonoBehaviour
{
    public string pointName; // �������� �����
    public int requiredLevel; // ��������� ������� ��� �����
    public PointType pointType; // ��� �����
    public LocationData locationData; // ������ ������� (��� �����)

    private void Update()
    {
        // ��������� �������
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HandleTouch(touch.position);
            }
        }

        // ��������� ������ ���� (��� ������������ � ���������)
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition);
        }
    }

    private void HandleTouch(Vector3 touchPosition)
    {
        // ����������� ������� ������� � ������� ����������
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2 touchWorldPosition = new Vector2(worldPosition.x, worldPosition.y);

        // ���������, ����� �� ������� � ���������
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
        // ����� ����� ��������� ����� ������
        Debug.Log("Loading town scene...");
        //SceneManager.LoadScene("TownScene");
    }

    private void StartBattle()
    {
        DungeonTransferData.LocationData = locationData;
        SceneManager.LoadScene("BattleScene");
    }

}