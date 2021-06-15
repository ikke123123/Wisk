using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Reward
{
    public Reward(int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException();

        this.quantity = quantity;
        typeOf = TypeOf.XP;
        id = "";
    }

    public Reward(string id, int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException();

        typeOf = TypeOf.Item;
        this.id = id;
        this.quantity = quantity;
    }

    public void GiveReward()
    {
        if (typeOf == TypeOf.Item)
        {
            InventoryManager.iM.Add(id, quantity);
        }
        //Implement XP Solution.
        else XPManager.xPM.Add(quantity);
    }

    public TypeOf typeOf;
    public string id;
    public int quantity;

    public enum TypeOf { XP, Item };
}

#if UNITY_EDITOR
[CustomEditor(typeof(Reward))]
[CanEditMultipleObjects]
public class RewardEditor : Editor
{
    SerializedProperty typeOf;
    SerializedProperty id;
    SerializedProperty quantity;

    private void OnEnable()
    {
        typeOf = serializedObject.FindProperty("typeOf");
        id = serializedObject.FindProperty("id");
        quantity = serializedObject.FindProperty("quantity");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(typeOf, new GUIContent("Type of", "The type of reward."));
        if (typeOf.enumValueIndex == (int)Reward.TypeOf.Item)
        {
            EditorGUILayout.PropertyField(id, new GUIContent("ID", "The ID of the item."));
        }
        EditorGUILayout.PropertyField(quantity, new GUIContent("Quantity", "Number of the item that must be given."));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif