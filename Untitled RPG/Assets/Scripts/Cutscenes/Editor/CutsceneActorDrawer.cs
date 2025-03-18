using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CutsceneActor))]
public class CutsceneActorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        _ = EditorGUI.BeginProperty(position, label, property);

        // Draw the label and get the rect for the remaining property fields.
        position = EditorGUI.PrefixLabel(position, label);

        // Define rects for the toggle and character field.
        Rect toggleRect = new(position.x, position.y, 70, position.height);
        Rect characterRect = new(position.x + 70, position.y, position.width - 70, position.height);

        SerializedProperty isPlayerProp = property.FindPropertyRelative("isPlayer");

        // Draw toggle for isPlayer.
        isPlayerProp.boolValue = GUI.Toggle(toggleRect, isPlayerProp.boolValue, "Is Player");

        // Draw the character field if isPlayer is false.
        if (!isPlayerProp.boolValue)
        {
            _ = EditorGUI.PropertyField(characterRect, property.FindPropertyRelative("character"), GUIContent.none);
        }

        // Apply property modifications after changes.
        _ = property.serializedObject.ApplyModifiedProperties();

        EditorGUI.EndProperty();
    }
}