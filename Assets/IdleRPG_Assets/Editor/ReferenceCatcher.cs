using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SAMPLETEXT
{


public class ReferenceCatcher : EditorWindow
{
        private Sprite oldSprite;
        private Sprite newSprite;

        [MenuItem("Tools/Update Sprite References")]
        public static void ShowWindow()
        {
            GetWindow<ReferenceCatcher>("Update Sprite References");
        }

        private void OnGUI()
        {
            GUILayout.Label("Update Sprite References", EditorStyles.boldLabel);

            oldSprite = (Sprite)EditorGUILayout.ObjectField("Old Sprite", oldSprite, typeof(Sprite), false);
            newSprite = (Sprite)EditorGUILayout.ObjectField("New Sprite", newSprite, typeof(Sprite), false);

            if (GUILayout.Button("Update References"))
            {
                if (oldSprite == null || newSprite == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please assign both the old sprite and the new sprite.", "OK");
                    return;
                }

                UpdateSpriteReferences(oldSprite, newSprite);
            }
        }

        private void UpdateSpriteReferences(Sprite oldSprite, Sprite newSprite)
        {
            int updatedCount = 0;

            // Find all GameObjects in the scene, including inactive ones
            var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var gameObject in allGameObjects)
            {
                // Ensure the GameObject belongs to the current scene (exclude prefabs in the editor)
                if (!gameObject.scene.IsValid())
                    continue;

                // Get all components on the GameObject
                var components = gameObject.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component == null) continue;

                    // Inspect serialized properties of the component
                    SerializedObject serializedObject = new SerializedObject(component);
                    SerializedProperty property = serializedObject.GetIterator();

                    while (property.NextVisible(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference &&
                            property.objectReferenceValue == oldSprite)
                        {
                            property.objectReferenceValue = newSprite;
                            serializedObject.ApplyModifiedProperties();
                            updatedCount++;
                        }
                    }
                }
            }

            // Log the results
            Debug.Log($"Updated {updatedCount} references from {oldSprite.name} to {newSprite.name}.");
            if (updatedCount == 0)
            {
                Debug.LogWarning($"No references to {oldSprite.name} were found in the scene.");
            }
        }
    }
}
