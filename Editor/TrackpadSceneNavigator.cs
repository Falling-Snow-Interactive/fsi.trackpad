using System;
using Fsi.Trackpad.Settings;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fsi.Trackpad
{
    [EditorTool("Trackpad Scene Navigator")]
    public class TrackpadSceneNavigator : EditorTool
    {
        private const float SWIPE_TOLERANCE = 0.001f;
        
        private float lastCameraSize = 1;
        private Vector2 lastMousePosition;

        private float height;

        private TrackpadSceneNavigatorSettings settings;

        [SerializeField]
        private GUIContent icon;
        public override GUIContent toolbarIcon => icon;
        
        private Texture defaultIcon;

        public enum NavigationMode
        {
            None = 0,
            PanXZ = 1,
            PanXY = 2,
            Orbit = 3,
            Zoom = 4
        }
        
        private NavigationMode mode = NavigationMode.None;
        public NavigationMode Mode => mode;
        
        private void OnEnable()
        {
            defaultIcon = AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.fallingsnowinteractive.trackpad/Editor/Art/Trackpad_XZ_EditorTool_Icon.png");
            icon = new GUIContent("Trackpad", defaultIcon, "FSI Trackpad");
        }

        public override void OnToolGUI(EditorWindow window)
        {
            settings ??= TrackpadSceneNavigatorSettings.GetOrCreateSettings();
            Event currentEvent = Event.current;

            if (IsSwipe(currentEvent))
            {
                CheckMode();
                DrawHandles();
                UpdateMode();
                
                SceneView.lastActiveSceneView.size = lastCameraSize;

                Vector3 heightVector = SceneView.lastActiveSceneView.pivot;
                heightVector.y = height;
                SceneView.lastActiveSceneView.pivot = heightVector;
            }
            else
            {
                lastMousePosition = currentEvent.mousePosition;
                lastCameraSize = SceneView.lastActiveSceneView.size;
            }
        }

        private bool IsSwipe(Event currentEvent)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                return false;
            }
        
            if (currentEvent.isMouse)
            {
                return false;
            }
        
            return currentEvent.mousePosition == lastMousePosition;
        }
        
        
        private void CheckMode()
        {
            Event currentEvent = Event.current;
            
            NavigationMode nextMode = NavigationMode.None;
            
            EventModifiers modifiers = currentEvent.modifiers;
            if (modifiers.HasFlag(EventModifiers.Alt))
            {
                height = 0;
                Vector3 pivot = SceneView.lastActiveSceneView.pivot;
                pivot.y = 0;
                SceneView.lastActiveSceneView.pivot = pivot;
            }
            else if (modifiers == EventModifiers.None)
            {
                nextMode = NavigationMode.PanXZ;
            }
            else if (modifiers.HasFlag(EventModifiers.Command))
            {
                nextMode = NavigationMode.Orbit;
            }
            else if (modifiers.HasFlag(EventModifiers.Shift))
            {
                nextMode = NavigationMode.PanXY;
            }
            else if (modifiers.HasFlag(EventModifiers.Control))
            {
                nextMode = NavigationMode.Zoom;
            }

            if (mode != nextMode)
            {
                Debug.Log($"{mode} -> {nextMode}");
                mode = nextMode;
            }
        }

        #region Draw Handles

        private void DrawHandles()
        {
            // do this based on the mode;
            switch (mode)
            {
                case NavigationMode.None:
                    break;
                case NavigationMode.PanXZ:
                    DrawPanXZHandles();
                    break;
                case NavigationMode.PanXY:
                    DrawPanXYHandles();
                    break;
                case NavigationMode.Orbit:
                    DrawOrbitHandles();
                    break;
                case NavigationMode.Zoom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (settings.ShowPivot)
            {
                Handles.color = Color.white;
                Handles.DrawSolidDisc(SceneView.lastActiveSceneView.pivot,
                                      SceneView.lastActiveSceneView.camera.transform.forward,
                                      0.1f);
            }
            
            if (settings.ShowCoordinates)
            {
                DrawCoordinates(SceneView.lastActiveSceneView.pivot);
            }
        }

        private void DrawPanXZHandles()
        {
            if (!settings.ShowPanPlanes)
            {
                return;
            }

            Vector3 forward = SceneView.lastActiveSceneView.camera.transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = SceneView.lastActiveSceneView.camera.transform.right;
            right.y = 0;
            right.Normalize();
            
            DrawPlane(SceneView.lastActiveSceneView.pivot, 
                      forward,
                      right,
                      settings.XPlaneColor,
                      settings.XPlaneOutlineColor);

            Vector3 point = SceneView.lastActiveSceneView.pivot;// - forward * 1f - right * 1f;
            float size = 0.6f;
            
            Handles.color = Color.blue;
            Handles.ArrowHandleCap(0, 
                                   point, 
                                   Quaternion.LookRotation(forward), 
                                   size,
                                   EventType.Repaint);
            
            Handles.color = Color.red;
            Handles.ArrowHandleCap(1, 
                                   point, 
                                   Quaternion.LookRotation(right), 
                                   size,
                                   EventType.Repaint);
        }
        
        private void DrawPanXYHandles()
        {
            if (!settings.ShowPanPlanes)
            {
                return;
            }
            
            Vector3 forward = SceneView.lastActiveSceneView.camera.transform.forward;
            forward.y = 0;
            forward.Normalize();
            
            Vector3 right = SceneView.lastActiveSceneView.camera.transform.right;
            right.y = 0;
            right.Normalize();
            
            DrawPlane(SceneView.lastActiveSceneView.pivot, 
                      Vector3.up,
                      right,
                      settings.YPlaneColor,
                      settings.YPlaneOutlineColor);
            
            Vector3 point = SceneView.lastActiveSceneView.pivot;// - forward * 1f - right * 1f;
            float size = 0.6f;
            
            Handles.color = Color.red;
            Handles.ArrowHandleCap(0, 
                                   point, 
                                   Quaternion.LookRotation(right), 
                                   size,
                                   EventType.Repaint);
            
            Handles.color = Color.green;
            Handles.ArrowHandleCap(1, 
                                   point, 
                                   Quaternion.LookRotation(Vector3.up), 
                                   size,
                                   EventType.Repaint);
        }

        private void DrawOrbitHandles()
        {
            if (settings.ShowRotateHandles)
            {
                Handles.RotationHandle(Quaternion.identity, SceneView.lastActiveSceneView.pivot);
            }
        }

        private void DrawPlane(Vector3 center, Vector3 forward, Vector3 right, Color faceColor, Color outlineColor)
        {
            Vector3 p0 = center + forward - right;
            Vector3 p1 = center + forward + right;
            Vector3 p2 = center - forward - right;
            Vector3 p3 = center - forward + right;
        
            Vector3[] verts = new Vector3[4];
            verts[0] = p0;
            verts[1] = p1;
            verts[2] = p3;
            verts[3] = p2;
            
            Handles.DrawSolidRectangleWithOutline(verts, faceColor, outlineColor);
        }

        private void DrawCoordinates(Vector3 center)
        {
            int labelSize = Mathf.RoundToInt(HandleUtility.GetHandleSize(center));
            float rise = HandleUtility.GetHandleSize(center) * 0.3f;
            GUIStyle style = new GUIStyle
                             {
                                 fontSize = 12,
                                 normal =
                                 {
                                     textColor = Color.black
                                 },
                                 alignment = TextAnchor.MiddleCenter,
                                 fontStyle = FontStyle.Normal,
                                 fixedWidth = labelSize,
                                 fixedHeight = labelSize
                             };

            string coordinates = $"({center.x:0.0}, {center.y:0.0}, {center.z:0.0})";
            Handles.Label(center - SceneView.lastActiveSceneView.camera.transform.up * rise, coordinates, style);
        }
    
        #endregion

        #region Mode Control

        private void UpdateMode()
        {
            Vector2 delta = Event.current.delta;
            if (delta.sqrMagnitude < SWIPE_TOLERANCE)
            {
                return;
            }
            
            switch (mode)
            {
                case NavigationMode.None:
                    break;
                case NavigationMode.PanXZ:
                    UpdatePanXZ();
                    break;
                case NavigationMode.PanXY:
                    UpdatePanXY();
                    break;
                case NavigationMode.Orbit:
                    UpdateOrbit();
                    break;
                case NavigationMode.Zoom:
                    UpdateZoom();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdatePanXZ()
        {
            Vector2 delta = Event.current.delta;
            
            Vector3 forward = SceneView.lastActiveSceneView.camera.transform.forward;
            forward.y = 0;
            forward.Normalize();
        
            Vector3 right = SceneView.lastActiveSceneView.camera.transform.right;
            right.y = 0;
            right.Normalize();
        
            Vector3 horizontal = right;
            Vector3 vertical = -forward;

            Vector3 move = horizontal * delta.x * settings.PanSensitivity * settings.PanSensitivityAxis.x
                           + vertical * delta.y * settings.PanSensitivity * settings.PanSensitivityAxis.y;
            
            SceneView.lastActiveSceneView.pivot += move;
        }

        private void UpdatePanXY()
        {
            Vector2 delta = Event.current.delta;
            
            Vector3 right = SceneView.lastActiveSceneView.camera.transform.right;
            right.y = 0;
            right.Normalize();
        
            Transform sceneCameraTransform = SceneView.lastActiveSceneView.camera.transform;
            Vector3 horizontal = sceneCameraTransform.right;
            Vector3 vertical = Vector3.down;

            Vector3 move = horizontal * delta.x * settings.PanSensitivity * settings.PanSensitivityAxis.x;
            height += vertical.y * delta.y * settings.PanSensitivity * settings.PanSensitivityAxis.y;
            SceneView.lastActiveSceneView.pivot += move; 
        }

        private void UpdateOrbit()
        {
            Vector2 delta = Event.current.delta;
            
            Vector3 newRotation = SceneView.lastActiveSceneView.rotation.eulerAngles;
            newRotation.x += delta.y * settings.RotateSensitivity * settings.RotateSensitivityAxis.y;
            newRotation.y += delta.x * settings.RotateSensitivity * settings.RotateSensitivityAxis.x;

            SceneView.lastActiveSceneView.LookAtDirect(SceneView.lastActiveSceneView.pivot, 
                                                       Quaternion.Euler(newRotation));
            SceneView.lastActiveSceneView.Repaint();
        }

        private void UpdateZoom()
        {
            float delta = Event.current.delta.y;
            lastCameraSize += delta * settings.ZoomSensitivity;
        }
        
        #endregion
    }
}