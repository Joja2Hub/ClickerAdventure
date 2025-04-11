using UnityEngine;

[CreateAssetMenu(menuName = "Game/Town Data")]
public class TownData : ScriptableObject
{
    public string townName;
    public Sprite background;
    public Vector2 shopPosition;
    public Vector2 guildPosition;
    public Vector2 innPosition;
}
