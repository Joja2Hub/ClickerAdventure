using UnityEngine;

[CreateAssetMenu(fileName = "LocationData", menuName = "Game/Location")]
[System.Serializable]
public class LocationData : ScriptableObject
{
    public string dungeonName;
    public Sprite background;
    public GameObject[] enemyPrefabs;
    public int waveCount = 3;
}
