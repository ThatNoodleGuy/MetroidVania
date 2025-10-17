using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class FlipSpritesTool : EditorWindow
{
    private List<Sprite> spritesToFlip = new List<Sprite>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Flip Sprites Tool")]
    public static void ShowWindow()
    {
        GetWindow<FlipSpritesTool>("Flip Sprites");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Flip Sprites", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Select sprites in the Project window, then click 'Add Selected Sprites'", MessageType.Info);
        
        if (GUILayout.Button("Add Selected Sprites"))
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj is Texture2D texture)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
                    foreach (Sprite sprite in sprites)
                    {
                        if (!spritesToFlip.Contains(sprite))
                        {
                            spritesToFlip.Add(sprite);
                        }
                    }
                }
                else if (obj is Sprite sprite)
                {
                    if (!spritesToFlip.Contains(sprite))
                    {
                        spritesToFlip.Add(sprite);
                    }
                }
            }
        }

        if (GUILayout.Button("Clear List"))
        {
            spritesToFlip.Clear();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Sprites to flip: {spritesToFlip.Count}");
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        for (int i = 0; i < spritesToFlip.Count; i++)
        {
            EditorGUILayout.ObjectField(spritesToFlip[i], typeof(Sprite), false);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        
        GUI.enabled = spritesToFlip.Count > 0;
        if (GUILayout.Button("Flip All Sprites Horizontally", GUILayout.Height(30)))
        {
            FlipSprites();
        }
        GUI.enabled = true;
    }

    private void FlipSprites()
    {
        if (EditorUtility.DisplayDialog("Confirm Flip", 
            $"Are you sure you want to flip {spritesToFlip.Count} sprite(s)? This cannot be undone!", 
            "Yes, Flip", "Cancel"))
        {
            int flipped = 0;
            foreach (Sprite sprite in spritesToFlip)
            {
                if (FlipSpriteTexture(sprite))
                {
                    flipped++;
                }
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Complete", $"Flipped {flipped} sprite(s) successfully!", "OK");
            spritesToFlip.Clear();
        }
    }

    private bool FlipSpriteTexture(Sprite sprite)
    {
        try
        {
            string path = AssetDatabase.GetAssetPath(sprite);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer == null) return false;

            // Make texture readable temporarily
            bool wasReadable = importer.isReadable;
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path);

            // Load texture
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) return false;

            // Create flipped texture
            Texture2D flipped = new Texture2D(texture.width, texture.height, texture.format, false);
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    flipped.SetPixel(texture.width - 1 - x, y, texture.GetPixel(x, y));
                }
            }
            flipped.Apply();

            // Save
            byte[] bytes = flipped.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);

            // Restore original settings
            importer.isReadable = wasReadable;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            DestroyImmediate(flipped);
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to flip sprite {sprite.name}: {e.Message}");
            return false;
        }
    }
}