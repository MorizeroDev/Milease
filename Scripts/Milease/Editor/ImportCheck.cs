#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEditor.Build;
using Assembly = System.Reflection.Assembly;

namespace Milease.Editor
{
    [InitializeOnLoad]
    public class ImportCheck : EditorWindow
    {
        private class PackageCheck
        {
            public string Assembly;
            public string TypeFullName;
            public string PackageName;
            public string PackageUrl;
            public bool IsUnityPackage;
            public bool IsOptional;
        }

        private static readonly List<PackageCheck> requirements = new List<PackageCheck>
        {
            new PackageCheck()
            {
                Assembly = "party.para.util.colors",
                TypeFullName = "Paraparty.Colors.ColorUtils",
                PackageName = "party.para.util.colors",
                PackageUrl = "https://github.com/ParaParty/ParaPartyUtil.git?path=Colors"
            },
            new PackageCheck()
            {
                Assembly = "party.para.util.unitypolyfill",
                TypeFullName = "Paraparty.UnityPolyfill.CollectionsPolyfill",
                PackageName = "party.para.util.unitypolyfill",
                PackageUrl = "https://github.com/ParaParty/ParaPartyUtil.git?path=UnityPolyfill"
            },
            new PackageCheck()
            {
                Assembly = "Unity.TextMeshPro",
                TypeFullName = "TMPro.TMP_Text",
                IsUnityPackage = true,
                IsOptional = true
            }
        };

        static ImportCheck()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.projectChanged += () =>
            {
                if (GetProblemCount() > 0)
                {
                    ShowWindow();
                }
            };
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (GetProblemCount() > 0)
                {
                    EditorApplication.ExitPlaymode();
                    EditorUtility.DisplayDialog("Milease Error",
                        "The current configuration of Milease is incorrect. Please complete the configuration as instructed in the Setup window.",
                        "OK");
                    ShowWindow();
                }
            }
        }
        
        [MenuItem("Milease/Configuration...")]
        public static void ShowWindow()
        {
            var window = GetWindow<ImportCheck>("Milease Setup");
            window.minSize = new Vector2(620f, 600f);
            window.maxSize = new Vector2(620f, Screen.height);
            window.ShowPopup();
        }

        private static int GetProblemCount()
        {
            var tick = 0;
#if MILEASE_ENABLE_CODEGEN && MILEASE_ENABLE_EXPRESSION
            tick++;
#endif
#if !MILEASE_ENABLE_CODEGEN && !MILEASE_ENABLE_EXPRESSION
            tick++;
#endif
#if MILEASE_ENABLE_EXPRESSION
            if (IsUsingIL2CPP() || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                tick++;
            }
#endif
            foreach (var item in requirements)
            {
                var typeName = Assembly.CreateQualifiedName(item.Assembly, item.TypeFullName);
                var type = Type.GetType(typeName);
                if (type == null && !item.IsOptional)
                {
                    tick++;
                }
            }

            return tick;
        }

        private enum AnimationImplSolution
        {
            None, CodeGen, Expression
        }
        
        private Texture HeadBar, Luvia;
        private Vector2 scrollPos;
        
        private bool scriptCompiling = false;
        
        private void OnEnable()
        {
            HeadBar = AssetDatabase.LoadAssetAtPath<Texture>(
                AssetDatabase.GUIDToAssetPath("62afa0cef89f3e84391bac8f4a307834"));
            Luvia = AssetDatabase.LoadAssetAtPath<Texture>(
                AssetDatabase.GUIDToAssetPath("0ea68c72f97f3e742ad5c0b249aa32e9"));
            
            CompilationPipeline.compilationStarted += StartCompiling;
            CompilationPipeline.compilationFinished += FinishCompiling;
        }

        private void OnDisable()
        {
            CompilationPipeline.compilationStarted -= StartCompiling;
            CompilationPipeline.compilationFinished -= FinishCompiling;
        }

        private void StartCompiling(object ctx)
        {
            scriptCompiling = true;
        }

        private void FinishCompiling(object ctx)
        {
            scriptCompiling = false;
        }
        
        private void OnGUI()
        {
            var titleStyle = new GUIStyle()
            {
                fontSize = 16,
                normal =
                {
                    textColor = Color.white
                },
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(0, 0, 0, 8)
            };
            var panelStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(5, 15, 10, 10),
                padding = new RectOffset(16, 16, 16, 16)
            };
            
            GUI.backgroundColor = Color.white;
            GUI.enabled = !scriptCompiling;
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            scrollPos.x = 0;
            
            GUILayout.Label(HeadBar, GUILayout.Width(620f), GUILayout.Height(115f));
            
#region Overview
            EditorGUILayout.BeginVertical(panelStyle);
            
            GUILayout.Label("Overview", titleStyle);
            
            GUILayout.Label($"Package Version: v3.2.0");

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Documents ↗", new GUIStyle(EditorStyles.linkLabel)))
            {
                Application.OpenURL("https://github.com/MorizeroDev/Milease/tree/master/~Document");
            }
            GUILayout.Space(10f);
            
            if (GUILayout.Button("Releases ↗", new GUIStyle(EditorStyles.linkLabel)))
            {
                Application.OpenURL("https://github.com/MorizeroDev/Milease/releases");
            }
            GUILayout.Space(10f);
            
            if (GUILayout.Button("Profile ↗", new GUIStyle(EditorStyles.linkLabel)))
            {
                Application.OpenURL("https://github.com/MorizeroDev");
            }
            GUILayout.Space(10f);
            
            if (GUILayout.Button("Our Game ↗", new GUIStyle(EditorStyles.linkLabel)))
            {
                Application.OpenURL("https://milthm.com");
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Separator();
            
            var problemCnt = GetProblemCount();
            if (problemCnt == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(EditorGUIUtility.IconContent("console.infoicon").image, new GUIStyle()
                {
                    fixedWidth = 24f,
                    fixedHeight = 24f
                });
                EditorGUILayout.LabelField("Everything has been setup up correctly.", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(EditorGUIUtility.IconContent("console.erroricon").image, new GUIStyle()
                {
                    fixedWidth = 24f,
                    fixedHeight = 24f
                });
                EditorGUILayout.LabelField($"There are still {problemCnt} issue(s) need to be solved.", new GUIStyle(EditorStyles.boldLabel)
                {
                    normal =
                    {
                        textColor = Color.red
                    }
                });
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.BeginHorizontal(new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 8, 8)
            });
            GUILayout.Label(Luvia, new GUIStyle()
            {
                fixedWidth = 48f,
                fixedHeight = 48f
            });
            
            if (GUILayout.Button("If you think this project is helpful to you, how about buying Luvia a cup of coffee?", new GUIStyle(EditorStyles.linkLabel)
                {
                    fontSize = 14,
                    margin = new RectOffset(16, 0, 9, 0),
                    wordWrap = true
                }))
            {
                if (EditorUtility.DisplayDialog("Thanks!", 
                        "Thank you so much for your generous support! Your support will help us develop the game and improve our open-source projects.", 
                        "Visit 爱发电",
                        "Visit Github Sponsor"))
                {
                    Application.OpenURL("https://afdian.com/a/morizero");
                }
                else
                {
                    Application.OpenURL("https://github.com/sponsors/MorizeroDev");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
#endregion

            EditorGUILayout.BeginVertical(panelStyle);

#region Animation Implementation
            GUILayout.Label("Animation Implementation", titleStyle);

            var solution = AnimationImplSolution.None;
            
#if MILEASE_ENABLE_EXPRESSION
            solution = AnimationImplSolution.Expression;
#endif
#if MILEASE_ENABLE_CODEGEN
            solution = AnimationImplSolution.CodeGen;
#endif
            if (scriptCompiling)
            {
                EditorGUILayout.LabelField("Waiting for the script compilation...");
            }
            else
            {
                var newSolution = EditorGUILayout.Popup("Solution", (int)solution, new string[]
                {
                    "None", "Code Generation (Recommend)", "Expression Tree"
                });
                if (solution == AnimationImplSolution.None)
                {
                    EditorGUILayout.HelpBox("Please select a solution to ensure Milease operates normally.", MessageType.Error);
                }
                else if (solution == AnimationImplSolution.Expression)
                {
                    EditorGUILayout.HelpBox("The expression tree solution does not work on some platforms that do not support JIT, such as iOS.", MessageType.Warning);
                    if (IsUsingIL2CPP())
                    {
                        EditorGUILayout.HelpBox("The expression tree solution is incompatible with IL2CPP.", MessageType.Error);
                    }
                }
               
#if MILEASE_ENABLE_CODEGEN
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Click the button below to regenerate access and compute source code for the current project’s members to improve runtime efficiency. " +
                                           "The member list can be set in the 'AccessorGenerationList.cs' and 'GenerationDisableList.cs' files within the 'Milease.CodeGen' folder.", EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("Generate codes..."))
                {
                    Il2CppCalFuncGenerator.Generate();
                }
#endif

                if (newSolution != (int)solution && newSolution != 0)
                {
                    RemoveScriptingDefineSymbols("MILEASE_ENABLE_EXPRESSION");
                    RemoveScriptingDefineSymbols("MILEASE_ENABLE_CODEGEN");
                    if (newSolution == (int)AnimationImplSolution.Expression)
                    {
                        AddScriptingDefineSymbols("MILEASE_ENABLE_EXPRESSION");
                    }
                    if (newSolution == (int)AnimationImplSolution.CodeGen)
                    {
                        scriptCompiling = true;
                        AssetDatabase.ImportPackage(AssetDatabase.GUIDToAssetPath("676ef914e26d06f428d9c1641f750c7e"), Directory.Exists(Path.Combine("Assets", "Milease.CodeGen")));
                        AddScriptingDefineSymbols("MILEASE_ENABLE_CODEGEN");
                    }
                    CompilationPipeline.RequestScriptCompilation();
                }
            }

            EditorGUILayout.EndVertical();
#endregion
            
#region Dependency Import
            EditorGUILayout.BeginVertical(panelStyle);
            
            GUILayout.Label("Dependency Import", titleStyle);
            
            GUILayout.Label("Status", EditorStyles.boldLabel);

            var missing = new List<PackageCheck>();
            foreach (var item in requirements)
            {
                var typeName = Assembly.CreateQualifiedName(item.Assembly, item.TypeFullName);
                var type = Type.GetType(typeName);
                EditorGUILayout.BeginHorizontal();
                var toggleValue = type != null;
                var newToggleValue = EditorGUILayout.ToggleLeft(item.Assembly, toggleValue);
                if (newToggleValue != toggleValue && !toggleValue)
                {
                    EditorUtility.DisplayDialog("Milease", "Please refer to the instructions below to import the dependency.", "OK");
                }
                EditorGUILayout.LabelField((toggleValue ? "Imported" : "Missing" + (item.IsOptional ? "(Optional)" : "")), new GUIStyle()
                {
                    normal =
                    {
                        textColor = toggleValue || item.IsOptional ? Color.white : Color.red
                    },
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight
                });
                EditorGUILayout.EndHorizontal();
                if (type == null)
                {
                    missing.Add(item);
                }
            }

            EditorGUILayout.Separator();
            
            if (missing.Count > 0)
            {
                GUILayout.Label("Action Required", new GUIStyle(EditorStyles.boldLabel)
                {
                    normal =
                    {
                        textColor = Color.red
                    }
                });
                var missingItems = missing.GroupBy(x => x.IsUnityPackage);
                foreach (var group in missingItems)
                {
                    if (group.Key)
                    {
                        GUILayout.Label("Open Window -> Package Manager, then import these 'Unity Registry' packages:");
                        foreach (var pack in group)
                        {
                            GUILayout.Label(pack.Assembly, EditorStyles.helpBox);
                        }
                    }
                    else
                    {
                        GUILayout.Label("Including these in `Packages/manifest.json`:");
                        var depString = string.Join("\n", group.Select(x => $"\"{x.PackageName}\": \"{x.PackageUrl}\","));
                        EditorGUILayout.TextArea(depString);
                        if (GUILayout.Button(GUIUtility.systemCopyBuffer == depString ? "Copied" : "Copy to Clipboard"))
                        {
                            GUIUtility.systemCopyBuffer = depString;
                        }
                    }
                }
            }
            else
            {
                GUILayout.Label("All dependencies have been imported.", EditorStyles.boldLabel);
            }
            
            EditorGUILayout.EndVertical();
#endregion
            
            EditorGUILayout.EndScrollView();
        }

#if UNITY_2021_3_OR_NEWER
        private static bool IsUsingIL2CPP()
        {
            return PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone) == ScriptingImplementation.IL2CPP ||
                   PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP ||
                   PlayerSettings.GetScriptingBackend(NamedBuildTarget.iOS) == ScriptingImplementation.IL2CPP;
        }

        private static void AddScriptingDefineSymbols(NamedBuildTarget target, string flag)
        {
            var symbols = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbols(target).Split(';'));
            symbols.Add(flag);
            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", symbols));
        }

        private static void AddScriptingDefineSymbols(string flag)
        {
            AddScriptingDefineSymbols(NamedBuildTarget.Android, flag);
            AddScriptingDefineSymbols(NamedBuildTarget.iOS, flag);
            AddScriptingDefineSymbols(NamedBuildTarget.Standalone, flag);
        }

        private static void RemoveScriptingDefineSymbols(NamedBuildTarget target, string flag)
        {
            var symbols = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbols(target).Split(';'));
            symbols.Remove(flag);
            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", symbols));
        }

        private static void RemoveScriptingDefineSymbols(string flag)
        {
            RemoveScriptingDefineSymbols(NamedBuildTarget.Android, flag);
            RemoveScriptingDefineSymbols(NamedBuildTarget.iOS, flag);
            RemoveScriptingDefineSymbols(NamedBuildTarget.Standalone, flag);
        }
#else
        private static bool IsUsingIL2CPP()
        {
            return PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone) == ScriptingImplementation.IL2CPP ||
                   PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP ||
                   PlayerSettings.GetScriptingBackend(BuildTargetGroup.iOS) == ScriptingImplementation.IL2CPP;
        }
        
        private static void AddScriptingDefineSymbols(BuildTargetGroup target, string flag)
        {
            var symbols = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';'));
            symbols.Add(flag);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", symbols));
        }

        private static void AddScriptingDefineSymbols(string flag)
        {
            AddScriptingDefineSymbols(BuildTargetGroup.Android, flag);
            AddScriptingDefineSymbols(BuildTargetGroup.iOS, flag);
            AddScriptingDefineSymbols(BuildTargetGroup.Standalone, flag);
        }

        private static void RemoveScriptingDefineSymbols(BuildTargetGroup target, string flag)
        {
            var symbols = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(target).Split(';'));
            symbols.Remove(flag);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", symbols));
        }

        private static void RemoveScriptingDefineSymbols(string flag)
        {
            RemoveScriptingDefineSymbols(BuildTargetGroup.Android, flag);
            RemoveScriptingDefineSymbols(BuildTargetGroup.iOS, flag);
            RemoveScriptingDefineSymbols(BuildTargetGroup.Standalone, flag);
        }
#endif
    }
}
#endif
