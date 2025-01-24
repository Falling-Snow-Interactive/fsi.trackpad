using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fsi.Trackpad.Settings
{
    public class TrackpadSceneNavigatorSettingsProvider : SettingsProvider
    {
        private const string SETTINGS_PATH = "Fsi/Trackpad";
        
        private SerializedObject serializedSettings;
        
        public TrackpadSceneNavigatorSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) 
            : base(path, scopes, keywords)
        {
        }
        
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new TrackpadSceneNavigatorSettingsProvider(SETTINGS_PATH, SettingsScope.Project);
        }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            serializedSettings = TrackpadSceneNavigatorSettings.GetSerializedSettings();
        }
        
        public override void OnGUI(string searchContext)
        {
            // EditorGUILayout.PropertyField(serializedSettings.FindProperty("prop"));
            
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("panSensitivity"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("panSensitivityAxis"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("rotateSensitivity"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("rotateSensitivityAxis"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("zoomSensitivity"));
            
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("handlesSize"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("showPivot"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("showPanPlanes"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("showRotateHandles"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("showCoordinates"));
            
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("xPlaneColor"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("xPlaneOutlineColor"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("yPlaneColor"));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty("yPlaneOutlineColor"));
            
            EditorGUILayout.Space(20);
            if (GUILayout.Button("Save"))
            {
                serializedSettings.ApplyModifiedProperties();
            }
            
            serializedSettings.ApplyModifiedProperties();
        } 
    }
}