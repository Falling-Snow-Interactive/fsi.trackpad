using UnityEditor;
using UnityEngine;

namespace Fsi.Trackpad.Settings
{
    public class TrackpadSceneNavigatorSettings : ScriptableObject
    {
        private const string RESOURCE_PATH = "Settings/TrackpadSceneNavigatorSettings.asset";
        private const string FULL_PATH = "Assets/Resources/" + RESOURCE_PATH + ".asset";

        [Header("Sensitivity")]

        [SerializeField]
        private float panSensitivity = 1f;
        public float PanSensitivity => panSensitivity;
        
        [SerializeField]
        private Vector2 panSensitivityAxis = Vector2.one;
        public Vector2 PanSensitivityAxis => panSensitivityAxis;
        
        [Space(10)]
        
        [SerializeField]
        private float rotateSensitivity = 1f;
        public float RotateSensitivity => rotateSensitivity;
        
        [SerializeField]
        private Vector2 rotateSensitivityAxis = Vector2.one;
        public Vector2 RotateSensitivityAxis => rotateSensitivityAxis;
        
        [Space(10)]
        
        [SerializeField]
        private float zoomSensitivity = 1f;
        public float ZoomSensitivity => zoomSensitivity;

        [Header("Handles")]

        [SerializeField]
        private bool showPivot = true;
        public bool ShowPivot => showPivot;

        [SerializeField]
        private bool showPanPlanes = true;
        public bool ShowPanPlanes => showPanPlanes;
        
        [SerializeField]
        private bool showRotateHandles = true;
        public bool ShowRotateHandles => showRotateHandles;
        
        [SerializeField]
        private bool showCoordinates = true;
        public bool ShowCoordinates => showCoordinates;

        [Header("Colors")]

        [SerializeField]
        private Color xPlaneColor = Color.green;
        public Color XPlaneColor => xPlaneColor;
        
        [SerializeField]
        private Color xPlaneOutlineColor = Color.green;
        public Color XPlaneOutlineColor => xPlaneOutlineColor;
        
        [SerializeField]
        private Color yPlaneColor = Color.green;
        public Color YPlaneColor => yPlaneColor;
        
        [SerializeField]
        private Color yPlaneOutlineColor = Color.green;
        public Color YPlaneOutlineColor => yPlaneOutlineColor;
        
        public static TrackpadSceneNavigatorSettings GetOrCreateSettings()
        {
            var settings = Resources.Load<TrackpadSceneNavigatorSettings>(RESOURCE_PATH);

            #if UNITY_EDITOR
            if (!settings)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                if (!AssetDatabase.IsValidFolder("Assets/Resources/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "Settings");
                }

                settings = CreateInstance<TrackpadSceneNavigatorSettings>();
                AssetDatabase.CreateAsset(settings, FULL_PATH);
                AssetDatabase.SaveAssets();
            }
            #endif

            return settings;
        }

        #if UNITY_EDITOR
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
        #endif
    }
}