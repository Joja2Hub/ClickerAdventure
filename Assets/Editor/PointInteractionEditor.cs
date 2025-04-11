using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PointInteraction))]
public class PointInteractionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PointInteraction script = (PointInteraction)target;

        // Стандартные поля
        script.pointName = EditorGUILayout.TextField("Point Name", script.pointName);
        script.requiredLevel = EditorGUILayout.IntField("Required Level", script.requiredLevel);
        script.pointType = (PointType)EditorGUILayout.EnumPopup("Point Type", script.pointType);

        // Поле GameObject — townUIPrefab
        script.townUIPrefab = (GameObject)EditorGUILayout.ObjectField("Town UI Prefab", script.townUIPrefab, typeof(GameObject), true);

        // Условная отрисовка
        if (script.pointType == PointType.Dungeon)
        {
            script.locationData = (LocationData)EditorGUILayout.ObjectField("Location Data", script.locationData, typeof(LocationData), false);
        }
        else if (script.pointType == PointType.Town)
        {
            script.townData = (TownData)EditorGUILayout.ObjectField("Town Data", script.townData, typeof(TownData), false);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }
    }
}
