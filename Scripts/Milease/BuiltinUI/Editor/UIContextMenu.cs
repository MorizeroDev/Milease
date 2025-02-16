#if UNITY_EDITOR
using Milease.Utils;
using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Milease.BuiltinUI.Editor
{
    public class UIContextMenu
    {
        [MenuItem("GameObject/Milease/Button")]
        private static void CreateHighLightMilButton(MenuCommand menuCommand)
            => CreateUIElement("0c0bfc89b3063c04d99fc506209cb427", menuCommand);
        
        [MenuItem("GameObject/Milease/HighLight Button")]
        private static void CreateMilButton(MenuCommand menuCommand)
            => CreateUIElement("b370f195c87ea5746bab53896a2e03b8", menuCommand);
        
        private static void CreateUIElement(string prefabGUID, MenuCommand menuCommand)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGUID));
            
            var selectedObject = menuCommand.context as GameObject;
            if (selectedObject == null || selectedObject.GetComponent<Canvas>() == null)
            {
                var canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                selectedObject = canvas.gameObject;
            }
            
            var instance = PrefabUtility.InstantiatePrefab(prefab, selectedObject.transform) as GameObject;
            if (instance != null)
            {
                instance.name = prefab.name;
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                foreach (var text in instance.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.font = TMP_Settings.defaultFontAsset;
                }
                
                Undo.RegisterCreatedObjectUndo(instance, "Create Milease Builtin UI Element");
                Selection.activeGameObject = instance;
            }
            else
            {
                LogUtils.Error("Failed to instantiate prefab.");
            }
        }
    }
}
#endif
