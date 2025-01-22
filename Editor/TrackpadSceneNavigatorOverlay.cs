using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fsi.Trackpad
{
    [Overlay(typeof(SceneView), "Trackpad", true)]
    public class TrackpadSceneNavigatorOverlay : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            VisualElement root = new VisualElement() { name = "My Toolbar Root" };
            
            Button test = new Button() { text = "Test" };
            root.Add(test);
            return root;
        }

        public override void OnCreated()
        {
            base.OnCreated();
            Debug.Log("OnCreated");
        }
    }
}