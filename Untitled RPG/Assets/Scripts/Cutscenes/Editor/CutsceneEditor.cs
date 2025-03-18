using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cutscene))]
public class CutsceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Cast the target to a Cutscene.
        Cutscene cutscene = target as Cutscene;
        if (cutscene == null)
        {
            base.OnInspectorGUI();
            return;
        }

        // First row of action buttons.
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Dialogue"))
            {
                cutscene.AddAction(new DialogueAction());
            }
            else if (GUILayout.Button("Move Actor"))
            {
                cutscene.AddAction(new MoveActorAction());
            }
            else if (GUILayout.Button("Turn Actor"))
            {
                cutscene.AddAction(new TurnActorAction());
            }
        }

        // Second row of action buttons.
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Teleport Object"))
            {
                cutscene.AddAction(new TeleportObjectAction());
            }
            else if (GUILayout.Button("Enable Object"))
            {
                cutscene.AddAction(new EnableObjectAction());
            }
            else if (GUILayout.Button("Disable Object"))
            {
                cutscene.AddAction(new DisableObjectAction());
            }
        }

        // Third row of action buttons.
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("NPC Interaction"))
            {
                cutscene.AddAction(new NPCInteractAction());
            }
            else if (GUILayout.Button("Fade In"))
            {
                cutscene.AddAction(new FadeInAction());
            }
            else if (GUILayout.Button("Fade Out"))
            {
                cutscene.AddAction(new FadeOutAction());
            }
        }

        // Draw the rest of the default inspector.
        base.OnInspectorGUI();
    }
}