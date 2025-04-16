using UnityEngine;

public abstract class QuestObjective : ScriptableObject
{
    public string description;
    public bool isCompleted;

    public abstract void Initialize();
    public abstract void CheckProgress();
}
