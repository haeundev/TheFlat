using System.Linq;
using EPOOutline;
using Proto.Util;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OutlineSetter))]
public class OutlineSetterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Set Outline"))
        {
            var outlineSetter = target as OutlineSetter;
            if (outlineSetter == default)
                return;
            var outlinables = outlineSetter.gameObject.GetComponentsInChildren<Outlinable>();
            outlinables.ForEach(p =>
            {
                //p.RenderStyle = RenderStyle.FrontBack;
                //p.BackParameters.Enabled = false;
                p.RenderStyle = RenderStyle.FrontBack;
                p.BackParameters.Enabled = true;
                p.BackParameters.Color = new Color(.01f, .01f, .01f);
                p.FrontParameters.Color = Color.white;
                p.UpdateVisibility();
            });
        }
        
        if (GUILayout.Button("Remove Outlines"))
        {
            var outlineSetter = target as OutlineSetter;
            if (outlineSetter == default)
                return;
            var outlinables = outlineSetter.gameObject.GetComponentsInChildren<Outlinable>();
            outlinables.ForEach(DestroyImmediate);
        }
        
        if (GUILayout.Button("Add Outlines"))
        {
            var outlineSetter = target as OutlineSetter;
            if (outlineSetter == default)
                return;
            var gos = outlineSetter.gameObject.GetComponentsInChildren<MeshRenderer>().Select(p => p.gameObject);
            foreach (var go in gos)
            {
                var p = go.AddComponent<Outlinable>();
                p.RenderStyle = RenderStyle.FrontBack;
                p.BackParameters.Enabled = true;
                p.BackParameters.Color = new Color(.01f, .01f, .01f);
                p.FrontParameters.Color = Color.white;
                p.UpdateVisibility();
            }
        }
        
        if (GUILayout.Button("Update Visibility"))
        {
            var outlineSetter = target as OutlineSetter;
            if (outlineSetter == default)
                return;
            var outlinables = outlineSetter.gameObject.GetComponentsInChildren<Outlinable>();
            outlinables.ForEach(p =>
            {
                p.UpdateVisibility();
            });
        }
    }
}