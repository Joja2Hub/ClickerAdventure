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
    public TownData townData;

    public GameObject townUIPrefab;

    private void Start()
    {
        townUIPrefab.gameObject.SetActive(false);
    }


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
        Debug.Log("��������");
        townUIPrefab.gameObject.SetActive(true);
    }



    private void StartBattle()
    {
        DungeonTransferData.LocationData = locationData;
        SceneManager.LoadScene("BattleScene");
    }

}