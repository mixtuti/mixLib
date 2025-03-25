using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mixLib
{
    public static class FolderSettingsProvider
    {
        private const string SettingsPath = "Assets/Resources/mixLib_FolderList.asset";
        private const string PackagePresetPath = "Packages/com.mixlib.core/Resources/Preset_FolderList.asset"; // パッケージ内のプリセットパス
        private static FolderList folderList;

        // デフォルトのフォルダパス
        private static readonly string[] defaultFolders = {
            "Audio",
            "Audio/BGM",
            "Audio/SE",
            "Sprites",
            "Prefabs",
            "Scenes",
            "Animations",
            "Materials",
            "Physics Materials",
            "Fonts",
            "Textures",
            "Resources",
            "Editor",
            "Plugins"
        };

        // プリセットを格納する変数
        private static FolderList selectedPreset;

        static FolderSettingsProvider()
        {
            // エディタの初期化時に一度だけデフォルトのフォルダリストを作成
            EditorApplication.delayCall += () =>
            {
                // パッケージ内のプリセットが存在しない場合にデフォルトを作成
                if (!File.Exists(PackagePresetPath))
                {
                    folderList = ScriptableObject.CreateInstance<FolderList>();
                    folderList.folders = new List<string>(defaultFolders);
                    AssetDatabase.CreateAsset(folderList, PackagePresetPath);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Default folder preset created at " + PackagePresetPath);
                }
            };
        }

        [SettingsProvider]
        public static SettingsProvider CreateFolderSettingsProvider()
        {
            var provider = new SettingsProvider("Project/mixLib/Folder Settings", SettingsScope.Project)
            {
                label = "Folder Settings",
                guiHandler = (searchContext) =>
                {
                    // 既に読み込まれている場合は再読み込みしない
                    if (folderList == null)
                    {
                        folderList = AssetDatabase.LoadAssetAtPath<FolderList>(SettingsPath);
                    }

                    // フォルダリストがない場合はデフォルトを作成
                    if (folderList == null)
                    {
                        folderList = ScriptableObject.CreateInstance<FolderList>();
                        folderList.folders = new List<string>(defaultFolders);
                        AssetDatabase.CreateAsset(folderList, SettingsPath);
                        AssetDatabase.SaveAssets();
                    }

                    // プリセット選択
                    EditorGUILayout.LabelField("Select Folder Preset");

                    // パッケージ内のプリセットを選択
                    string[] presetPaths = AssetDatabase.FindAssets("t:FolderList", new[] { "Packages/com.mixlib.core/Resources" })
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .ToArray();

                    // プリセットリストから選択
                    selectedPreset = (FolderList)EditorGUILayout.ObjectField("Preset", selectedPreset, typeof(FolderList), false);

                    // プリセットリストが選ばれた場合、そのプリセットをロードするボタン
                    if (selectedPreset != null && GUILayout.Button("Load Selected Preset"))
                    {
                        folderList = selectedPreset;
                        Debug.Log($"Loaded preset from {AssetDatabase.GetAssetPath(selectedPreset)}");
                    }

                    // フォルダリストがある場合は、編集可能にする
                    SerializedObject serializedObject = new SerializedObject(folderList);
                    SerializedProperty foldersProperty = serializedObject.FindProperty("folders");

                    EditorGUILayout.PropertyField(foldersProperty, true);
                    serializedObject.ApplyModifiedProperties(); // 変更を適用

                    // フォルダ作成ボタン
                    if (GUILayout.Button("Create Folders"))
                    {
                        CreateFolders();
                    }

                    // 編集した内容を保存
                    if (GUI.changed)
                    {
                        // 変更があった場合はフォルダリストを「Dirty」状態に設定
                        EditorUtility.SetDirty(folderList);
                        AssetDatabase.SaveAssets(); // アセットを保存
                        AssetDatabase.Refresh(); // アセットをリフレッシュ
                    }
                },
            };
            return provider;
        }

        // 階層的にフォルダを作成するメソッド
        private static void CreateFolders()
        {
            foreach (var folder in folderList.folders)
            {
                string[] folderPathParts = folder.Split('/');
                string currentPath = "Assets";

                foreach (var part in folderPathParts)
                {
                    currentPath = Path.Combine(currentPath, part);

                    // フォルダがまだ存在しない場合に作成
                    if (!AssetDatabase.IsValidFolder(currentPath))
                    {
                        AssetDatabase.CreateFolder(Path.GetDirectoryName(currentPath), part);
                        Debug.Log($"Created folder: {currentPath}");
                    }
                    else
                    {
                        Debug.Log($"Folder already exists: {currentPath}");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // 上部メニューから呼び出されるメソッド
        [MenuItem("mixLib/Create Folders")]
        private static void CreateFoldersMenu()
        {
            CreateFolders();
            Debug.Log("Folders created from the menu!");
        }
    }
}
