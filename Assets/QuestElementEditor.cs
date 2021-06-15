using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(QuestElement))]
[CanEditMultipleObjects]
public class QuestElementEditor : Editor
{
    SerializedProperty questID;
    SerializedProperty questElementID;
    SerializedProperty questElementName;

    SerializedProperty dependency;

    SerializedProperty useRewards;
    SerializedProperty rewards;

    SerializedProperty useDialogue;
    SerializedProperty dialogue;

    SerializedProperty useLocationTrigger;
    SerializedProperty locationTriggerPrefab;
    SerializedProperty locationTrigger;

    SerializedProperty questUIManager;
    SerializedProperty status;

    private void OnEnable()
    {
        questID = serializedObject.FindProperty("questID");
        questElementID = serializedObject.FindProperty("questElementID");
        questElementName = serializedObject.FindProperty("questElementName");

        dependency = serializedObject.FindProperty("dependency");

        //Rewards
        useRewards = serializedObject.FindProperty("useRewards");
        rewards = serializedObject.FindProperty("rewards");

        //Dialogue
        useDialogue = serializedObject.FindProperty("useDialogue");
        dialogue = serializedObject.FindProperty("dialogue");

        //Location Trigger
        useLocationTrigger = serializedObject.FindProperty("useLocationTrigger");
        locationTriggerPrefab = serializedObject.FindProperty("locationTriggerPrefab");
        locationTrigger = serializedObject.FindProperty("locationTrigger");

        //Debug
        questUIManager = serializedObject.FindProperty("questUIManager");
        status = serializedObject.FindProperty("status");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(questID, new GUIContent("Quest ID", "ID of the parent quest."));
        EditorGUILayout.PropertyField(questElementID, new GUIContent("Quest Element ID", "ID of this quest element."));
        EditorGUILayout.PropertyField(questElementName, new GUIContent("Quest Element Name", "Name of this quest."));

        EditorGUILayout.PropertyField(dependency, new GUIContent("Dependency", "The quest element's with other quests."));

        //Rewards
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Rewards", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useRewards, new GUIContent("Use Rewards", "Rewards will be given upon completing this quest element."));
        if (useRewards.boolValue)
            EditorGUILayout.PropertyField(rewards, new GUIContent("Rewards"), true);

        //Dialogue
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Dialogue", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useDialogue, new GUIContent("Use Dialogue", "The dialogue will play when the quest element is active."));
        if (useDialogue.boolValue)
            EditorGUILayout.PropertyField(dialogue, new GUIContent("Dialogue Blocks"), true);


        //Location trigger
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Location Trigger", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useLocationTrigger, new GUIContent("Use Location Trigger"));
        if (useLocationTrigger.boolValue)
        {
            EditorGUILayout.PropertyField(locationTriggerPrefab, new GUIContent("Location Trigger Prefab"));
            EditorGUILayout.PropertyField(locationTrigger, new GUIContent("Location Trigger Data"), true);
        }

        //Debug
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(new GUIContent("Quest Status: " +((Status)status.enumValueIndex).ToString(), "Current quest status."));
        EditorGUILayout.HelpBox(new GUIContent("Current QuestUI Manager Status: " + questUIManager.displayName, "The quest UI element selected."));


        serializedObject.ApplyModifiedProperties();
    }
}
#endif