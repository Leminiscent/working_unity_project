using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpriteToAnimation
{
    [MenuItem("Assets/Sprites to Animations")]
    private static void CreateAnimationsFromSprites()
    {
        string[] guids = Selection.assetGUIDs;

        List<Texture2D> assets = new();
        foreach (string guid in guids)
        {
            GuidToList(guid, assets);
        }

        if (assets.Count == 0)
        {
            return;
        }

        foreach (Texture2D asset in assets)
        {
            CreateAnimation(asset);
        }
    }

    private static void CreateAnimation(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (sprites.Length == 0)
        {
            return;
        }

        // Create animations folder.
        string animationPath = Path.Combine(Path.GetDirectoryName(path), "Animations");
        if (!Directory.Exists(animationPath))
        {
            Directory.CreateDirectory(animationPath);
        }

        // Create animation asset.
        AnimationClip clip = new();

        // Create binding curve.
        EditorCurveBinding curveBinding = new()
        {
            path = "Base",
            propertyName = "m_Sprite",
            type = typeof(SpriteRenderer)
        };

        // Create keyframes.
        float frameRate = 6f;
        List<ObjectReferenceKeyframe> objectReferences = new();
        for (int i = 0; i <= sprites.Length; i++)
        {
            // Allows adding a hold frame to make sure the last sprite frame doesn't get cut off too soon.
            int index = i;
            if (index >= sprites.Length)
            {
                index = sprites.Length - 1;
            }

            Sprite sprite = sprites[index];
            ObjectReferenceKeyframe newKeyframe = new()
            {
                value = sprite,
                time = i * (1f / frameRate)
            };

            objectReferences.Add(newKeyframe);
        }
        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, objectReferences.ToArray());

        // Save.
        AssetDatabase.CreateAsset(clip, Path.Combine(animationPath, texture.name + ".anim"));
    }

    private static void GuidToList(string guid, List<Texture2D> assetList)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (asset == null)
        {
            return;
        }

        if (asset is Texture2D texture)
        {
            assetList.Add(texture);
        }
    }
}