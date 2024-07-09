﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

[CustomEditor(typeof(Readme))]
[InitializeOnLoad]
public class ReadmeEditor : Editor
{
    static string s_ShowedReadmeSessionStateName = "ReadmeEditor.showedReadme";
    static string s_ReadmeSourceDirectory = "Assets/TutorialInfo";
    const float k_Space = 16f;

    static ReadmeEditor()
    {
        EditorApplication.delayCall += SelectReadmeAutomatically;
    }

    static void RemoveTutorial()
    {
        if (EditorUtility.DisplayDialog("Remove Readme Assets",
            $"All contents under {s_ReadmeSourceDirectory} will be removed, are you sure you want to proceed?",
            "Proceed",
            "Cancel"))
        {
            if (Directory.Exists(s_ReadmeSourceDirectory))
            {
                FileUtil.DeleteFileOrDirectory(s_ReadmeSourceDirectory);
                FileUtil.DeleteFileOrDirectory(s_ReadmeSourceDirectory + ".meta");
            }
            else
            {
                Debug.Log($"Could not find the Readme folder at {s_ReadmeSourceDirectory}");
            }

            var readmeAsset = SelectReadme();
            if (readmeAsset != null)
            {
                var path = AssetDatabase.GetAssetPath(readmeAsset);
                FileUtil.DeleteFileOrDirectory(path + ".meta");
                FileUtil.DeleteFileOrDirectory(path);
            }

            AssetDatabase.Refresh();
        }
    }

    static void SelectReadmeAutomatically()
    {
        if (!SessionState.GetBool(s_ShowedReadmeSessionStateName, false))
        {
            var readme = SelectReadme();
            SessionState.SetBool(s_ShowedReadmeSessionStateName, true);

            if (readme && !readme.loadedLayout)
            {
                LoadLayout();
                readme.loadedLayout = true;
            }
        }
    }

    static void LoadLayout()
    {
        var assembly = typeof(EditorApplication).Assembly;
        var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
        var method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
        method.Invoke(null, new object[] { Path.Combine(Application.dataPath, "TutorialInfo/Layout.wlt"), false });
    }

    static Readme SelectReadme()
    {
        var ids = AssetDatabase.FindAssets("Readme t:Readme");
        if (ids.Length == 1)
        {
            var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
            Selection.objects = new UnityEngine.Object[] { readmeObject };
            return (Readme)readmeObject;
        }
        else
        {
            Debug.Log("Couldn't find a readme");
            return null;
        }
    }

    protected override void OnHeaderGUI()
    {
        var readme = (Readme)target;
        Init();

        var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

        GUILayout.BeginHorizontal("In BigTitle");
        {
            if (readme.icon != null)
            {
                GUILayout.Space(k_Space);
                GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
            }
            GUILayout.Space(k_Space);
            GUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(readme.title, TitleStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var readme = (Readme)target;
        Init();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("loadedLayout"));

        SerializedProperty sections = serializedObject.FindProperty("sections");
        for (int i = 0; i < sections.arraySize; i++)
        {
            SerializedProperty section = sections.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(section.FindPropertyRelative("heading"));
            EditorGUILayout.PropertyField(section.FindPropertyRelative("text"));
            EditorGUILayout.PropertyField(section.FindPropertyRelative("linkText"));
            EditorGUILayout.PropertyField(section.FindPropertyRelative("url"));
            GUILayout.Space(k_Space);
        }

        if (GUILayout.Button("Remove Readme Assets", ButtonStyle))
        {
            RemoveTutorial();
        }

        serializedObject.ApplyModifiedProperties();
    }

    bool m_Initialized;

    GUIStyle LinkStyle => m_LinkStyle;
    [SerializeField] GUIStyle m_LinkStyle;

    GUIStyle TitleStyle => m_TitleStyle;
    [SerializeField] GUIStyle m_TitleStyle;

    GUIStyle HeadingStyle => m_HeadingStyle;
    [SerializeField] GUIStyle m_HeadingStyle;

    GUIStyle BodyStyle => m_BodyStyle;
    [SerializeField] GUIStyle m_BodyStyle;

    GUIStyle ButtonStyle => m_ButtonStyle;
    [SerializeField] GUIStyle m_ButtonStyle;

    void Init()
    {
        if (m_Initialized) return;

        m_BodyStyle = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            fontSize = 14,
            richText = true
        };

        m_TitleStyle = new GUIStyle(m_BodyStyle)
        {
            fontSize = 26
        };

        m_HeadingStyle = new GUIStyle(m_BodyStyle)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 18
        };

        m_LinkStyle = new GUIStyle(m_BodyStyle)
        {
            wordWrap = false,
            normal = { textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f) },
            stretchWidth = false
        };

        m_ButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            fontStyle = FontStyle.Bold
        };

        m_Initialized = true;
    }

    bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

        Handles.BeginGUI();
        Handles.color = LinkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, LinkStyle);
    }
}
