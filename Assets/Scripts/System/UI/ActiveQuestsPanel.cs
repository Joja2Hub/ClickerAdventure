using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveQuestsPanel : MonoBehaviour
{
    public Transform questListParent;
    public GameObject questPrefab;

    public void RefreshActiveQuests()
    {
        if (QuestManager.Instance == null || questListParent == null || questPrefab == null)
            return;

        ClearList();

        AddSection("Adventure quests", "Progress in the game world");
        AddQuestItems(QuestManager.Instance.activeQuests);
        if (QuestManager.Instance.activeQuests.Count == 0)
            AddEmptyState("No adventure quests are active.");

        AddSection("Real-life tasks", "Complete routines and wait for parent approval");
        AddDailyRoutineSummary();
        AddExternalQuestItems(QuestManager.Instance.externalQuestDatas);
        if (QuestManager.Instance.externalQuestDatas.Count == 0)
            AddEmptyState("No real-life tasks right now.");
    }

    private void OnEnable()
    {
        RefreshActiveQuests();
    }

    private void ClearList()
    {
        foreach (Transform child in questListParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddQuestItems(List<QuestData> quests)
    {
        foreach (var quest in quests)
        {
            GameObject questGO = Instantiate(questPrefab, questListParent);
            ActiveQuestUIItem uiItem = questGO.GetComponent<ActiveQuestUIItem>();
            uiItem.Setup(quest);
        }
    }

    private void AddExternalQuestItems(List<ExternalQuestData> quests)
    {
        foreach (var extQuest in quests)
        {
            GameObject questGO = Instantiate(questPrefab, questListParent);
            ActiveQuestUIItem uiItem = questGO.GetComponent<ActiveQuestUIItem>();
            uiItem.SetupExternal(extQuest);
        }
    }

    private void AddSection(string title, string subtitle)
    {
        GameObject section = CreateListTextObject("SectionHeader");
        Image background = section.AddComponent<Image>();
        background.color = new Color(0.08f, 0.1f, 0.13f, 0.78f);

        VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 12, 12);
        layout.spacing = 2f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        AddLayoutElement(section, 118f);

        TextMeshProUGUI titleText = CreateText(section.transform, title, 34, FontStyles.Bold, new Color(1f, 0.92f, 0.62f, 1f));
        titleText.alignment = TextAlignmentOptions.Left;

        TextMeshProUGUI subtitleText = CreateText(section.transform, subtitle, 22, FontStyles.Normal, new Color(0.82f, 0.86f, 0.92f, 1f));
        subtitleText.alignment = TextAlignmentOptions.Left;
    }

    private void AddEmptyState(string text)
    {
        GameObject empty = CreateListTextObject("EmptyState");
        Image background = empty.AddComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.06f);
        AddLayoutElement(empty, 92f);

        TextMeshProUGUI label = CreateText(empty.transform, text, 24, FontStyles.Italic, new Color(0.75f, 0.79f, 0.86f, 1f));
        Stretch(label.GetComponent<RectTransform>());
        label.alignment = TextAlignmentOptions.Center;
    }

    private void AddDailyRoutineSummary()
    {
        DailyRoutineProgress routineProgress = DailyRoutineProgress.Instance;
        routineProgress.ResetForCurrentDayIfNeeded();

        GameObject card = CreateListTextObject("DailyRoutineSummary");
        Image background = card.AddComponent<Image>();
        background.color = new Color(0.09f, 0.14f, 0.16f, 0.96f);
        AddLayoutElement(card, 156f);

        VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 14, 16);
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateText(
            card.transform,
            $"Daily routine {routineProgress.CompletedToday}/{routineProgress.DailyGoal}",
            28,
            FontStyles.Bold,
            new Color(0.7f, 1f, 0.78f, 1f));
        title.alignment = TextAlignmentOptions.Left;

        string bonusStatus = routineProgress.HasClaimedGoalBonusToday
            ? "Daily bonus claimed"
            : "Finish today's goal for a streak bonus";

        TextMeshProUGUI subtitle = CreateText(
            card.transform,
            $"Streak: {routineProgress.CurrentStreak} days  |  {bonusStatus}",
            20,
            FontStyles.Normal,
            new Color(0.82f, 0.9f, 0.92f, 1f));
        subtitle.alignment = TextAlignmentOptions.Left;

        GameObject bar = CreateListTextObject("DailyRoutineProgressBar");
        bar.transform.SetParent(card.transform, false);
        Image barBackground = bar.AddComponent<Image>();
        barBackground.color = new Color(0.04f, 0.06f, 0.07f, 1f);
        AddLayoutElement(bar, 18f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(bar.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.25f, 0.86f, 0.46f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = routineProgress.GoalProgress;
        Stretch(fill.GetComponent<RectTransform>());
    }

    private GameObject CreateListTextObject(string name)
    {
        GameObject listObject = new GameObject(name, typeof(RectTransform));
        listObject.transform.SetParent(questListParent, false);
        return listObject;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, int size, FontStyles style, Color color)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = color;
        label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }

    private void AddLayoutElement(GameObject target, float preferredHeight)
    {
        LayoutElement layoutElement = target.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;
        layoutElement.flexibleWidth = 1f;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
