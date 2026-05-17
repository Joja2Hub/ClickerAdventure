using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class QuestObjectiveProgress
{
    public string objectiveType;
    public string description;
    public int currentCount;
    public int targetCount = 1;
    public bool isCompleted;

    public string ProgressText => targetCount > 1
        ? $"{description} ({currentCount}/{targetCount})"
        : description;
}

public class QuestProgress
{
    public QuestData QuestData { get; }
    public List<QuestObjectiveProgress> Objectives { get; } = new List<QuestObjectiveProgress>();

    public bool IsCompleted => Objectives.Count > 0 && Objectives.All(objective => objective.isCompleted);

    public QuestProgress(QuestData questData)
    {
        QuestData = questData;

        if (questData == null || questData.objectives == null)
            return;

        foreach (var objective in questData.objectives)
        {
            if (objective == null)
                continue;

            var progress = new QuestObjectiveProgress
            {
                objectiveType = objective.GetType().Name,
                description = objective.description,
                targetCount = 1,
                currentCount = 0,
                isCompleted = false
            };

            if (objective is KillEnemyObjective killEnemyObjective)
                progress.targetCount = Math.Max(1, killEnemyObjective.targetCount);

            Objectives.Add(progress);
        }
    }

    public void RegisterEnemyKilled(EnemyData enemyData)
    {
        foreach (var objective in Objectives)
        {
            if (objective.isCompleted || objective.objectiveType != nameof(KillEnemyObjective))
                continue;

            objective.currentCount++;
            if (objective.currentCount >= objective.targetCount)
                objective.isCompleted = true;
        }
    }

    public void ApplySaveData(QuestProgressSaveData saveData)
    {
        if (saveData?.objectives == null)
            return;

        for (int i = 0; i < Objectives.Count && i < saveData.objectives.Count; i++)
        {
            Objectives[i].currentCount = saveData.objectives[i].currentCount;
            Objectives[i].isCompleted = saveData.objectives[i].isCompleted;
        }
    }

    public QuestProgressSaveData ToSaveData(string questId)
    {
        return new QuestProgressSaveData
        {
            questId = questId,
            objectives = Objectives.Select(objective => new QuestObjectiveProgressSaveData
            {
                currentCount = objective.currentCount,
                isCompleted = objective.isCompleted
            }).ToList()
        };
    }
}

[Serializable]
public class QuestObjectiveProgressSaveData
{
    public int currentCount;
    public bool isCompleted;
}

[Serializable]
public class QuestProgressSaveData
{
    public string questId;
    public List<QuestObjectiveProgressSaveData> objectives = new List<QuestObjectiveProgressSaveData>();
}

[Serializable]
public class QuestManagerSaveData
{
    public List<string> completedQuestIds = new List<string>();
    public List<QuestProgressSaveData> activeQuests = new List<QuestProgressSaveData>();
}
