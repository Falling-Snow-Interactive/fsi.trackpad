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

        private float height = 0;

        private TrackpadSceneNavigatorSettings settings;

        public override void OnToolGUI(EditorWindow window)
        {
            settings ??= TrackpadSceneNavigatorSettings.GetOrCreateSettings();
            
            Event currentEvent = Event.current;
            lastMousePosition = currentEvent.mousePosition;

            Vector2 delta = currentEvent.delta;

            Vector3 pivot = SceneView.lastActiveSceneView.pivot;
        
            bool up = currentEvent.shift;
            bool rotate = currentEvent.command;
            bool reset = currentEvent.alt;
            bool zoom = currentEvent.control;
            
            // Reset height check first
            if (reset)
            {
                height = 0;
                pivot.y = height;
                SceneView.lastActiveSceneView.pivot = pivot;
            }
        
            // Draw Handles
            DrawHandles(pivot, rotate, up);
        
            // Handle Trackpad
            bool isSwipe = IsSwipe(currentEvent);
            HandleTrackpad(pivot, delta, isSwipe, zoom, rotate, up);
        
            if(!isSwipe)
            {
                base.OnToolGUI(window);
                lastCameraSize = SceneView.lastActiveSceneView.size;
                lastMousePosition = currentEvent.mousePosition;
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
    
        #region Trackpad Controls

        private void HandleTrackpad(Vector3 pivot, Vector2 delta, bool isSwipe, bool zoom, bool rotate, bool up)
        {
            if (delta.sqrMagnitude < SWIPE_TOLERANCE)
            {
                return;
            }
        
            if (isSwipe)
            {
                pivot.y = height;
                SceneView.lastActiveSceneView.pivot = pivot;
            
                if (zoom)
                {
                    Zoom(delta.y);
                }
                else if (rotate)
                {
                    Rotate(delta);
                }
                else if (up)
                {
                    PanUp(delta);
                }
                else
                {
                    PanForward(delta);
                }
            
                SceneView.lastActiveSceneView.size = lastCameraSize;
            }
        }

        private void PanForward(Vector2 delta)
        {
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
    
        private void PanUp(Vector2 delta)
        {
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

        private void Rotate(Vector2 delta)
        {
            Vector3 newRotation = SceneView.lastActiveSceneView.rotation.eulerAngles;
            newRotation.x += delta.y * settings.RotateSensitivity * settings.RotateSensitivityAxis.y;
            newRotation.y += delta.x * settings.RotateSensitivity * settings.RotateSensitivityAxis.x;

            SceneView.lastActiveSceneView.LookAtDirect(SceneView.lastActiveSceneView.pivot, 
                                                       Quaternion.Euler(newRotation));
            SceneView.lastActiveSceneView.Repaint();
        }

        private void Zoom(float delta)
        {
            lastCameraSize += delta * settings.ZoomSensitivity;
        }
    
        #endregion

        #region Draw Handles

        private void DrawHandles(Vector3 pivot, bool rotate, bool up)
        {
            Vector3 forward = SceneView.lastActiveSceneView.camera.transform.forward;
            forward.y = 0;
            forward.Normalize();
        
            Vector3 right = SceneView.lastActiveSceneView.camera.transform.right;
            right.y = 0;
            right.Normalize();
        
            Vector3 upDir = SceneView.lastActiveSceneView.camera.transform.up;
            upDir.y = 0;
            upDir.Normalize();

            if (settings.ShowPivot)
            {
                Handles.DrawSolidDisc(SceneView.lastActiveSceneView.pivot,
                                      forward,
                                      0.1f);
            }

            if (settings.ShowCoordinates)
            {
                DrawCoordinates(pivot);
            }

            if (rotate)
            {
                if (settings.ShowRotateHandles)
                {
                    Handles.RotationHandle(Quaternion.identity, SceneView.lastActiveSceneView.pivot);
                }
            }
            else if (up && settings.ShowPanPlanes)
            {
                DrawPlane(SceneView.lastActiveSceneView.pivot, 
                          Vector3.up,
                          right,
                          settings.YPlaneColor,
                          settings.YPlaneOutlineColor);
            }
            else if(settings.ShowPanPlanes)
            {
                DrawPlane(SceneView.lastActiveSceneView.pivot, 
                          forward,
                          right,
                          settings.XPlaneColor,
                          settings.XPlaneOutlineColor);
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
            Handles.Label(center + SceneView.lastActiveSceneView.camera.transform.up * rise, coordinates, style);
        }
    
        #endregion
    }
}