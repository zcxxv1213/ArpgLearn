using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(GFHexGrid))]
public class GFHexGridEditor : Editor {
	
	private GFHexGrid hg;
	private bool showDrawSettings;
	
	void OnEnable(){
		hg = target as GFHexGrid;
		showDrawSettings = EditorPrefs.HasKey("GFHexGridShowDraw") ? EditorPrefs.GetBool("GFHexGridShowDraw") : true;
	}
	
	void OnDisable(){
		EditorPrefs.SetBool("GFHexGridShowDraw", showDrawSettings);
	}
		
	public override void OnInspectorGUI(){
		hg.relativeSize = EditorGUILayout.Toggle("Relative Size", hg.relativeSize);
		hg.size = EditorGUILayout.Vector3Field("Size", hg.size);
		hg.radius = EditorGUILayout.FloatField("Radius", hg.radius);
		hg.depth = EditorGUILayout.FloatField("Depth", hg.depth);
		hg.gridPlane = (GFGrid.GridPlane) EditorGUILayout.EnumPopup("Grid Plane", hg.gridPlane);
		hg.hexSideMode = (GFHexGrid.HexOrientation) EditorGUILayout.EnumPopup("Hex Grid Orientation", hg.hexSideMode);
		
		EditorGUILayout.Space();
		
		GUILayout.Label("Axis Colors");
		
		EditorGUILayout.BeginHorizontal();
		++EditorGUI.indentLevel;
		hg.axisColors.x = EditorGUILayout.ColorField(hg.axisColors.x);
		hg.axisColors.y = EditorGUILayout.ColorField(hg.axisColors.y);
		hg.axisColors.z = EditorGUILayout.ColorField(hg.axisColors.z);
		--EditorGUI.indentLevel;
		EditorGUILayout.EndHorizontal();
		
		hg.useSeparateRenderColor = EditorGUILayout.Foldout(hg.useSeparateRenderColor, "Use Separate Render Color");
		if(hg.useSeparateRenderColor){
			EditorGUILayout.BeginHorizontal();
			++EditorGUI.indentLevel;
			hg.renderAxisColors.x = EditorGUILayout.ColorField(hg.renderAxisColors.x);
			hg.renderAxisColors.y = EditorGUILayout.ColorField(hg.renderAxisColors.y);
			hg.renderAxisColors.z = EditorGUILayout.ColorField(hg.renderAxisColors.z);
			--EditorGUI.indentLevel;
		EditorGUILayout.EndHorizontal();
		}
		
		hg.vertexColor = EditorGUILayout.ColorField("Vertex Color", hg.vertexColor);
		
		EditorGUILayout.Space();
		
		showDrawSettings = EditorGUILayout.Foldout(showDrawSettings, "Draw & Render Settings");
		++EditorGUI.indentLevel;
		if(showDrawSettings){
			hg.renderGrid = EditorGUILayout.Toggle("Render Grid", hg.renderGrid);
			hg.gridStyle = (GFHexGrid.HexGridShape)EditorGUILayout.EnumPopup("Grid Style", hg.gridStyle);
			
			hg.useCustomRenderRange = EditorGUILayout.Foldout(hg.useCustomRenderRange, "Custom Rendering Range");
			if(hg.useCustomRenderRange){
				hg.renderFrom = Vector3.Min(hg.renderTo, EditorGUILayout.Vector3Field("Render From", hg.renderFrom));
				hg.renderTo = Vector3.Max(hg.renderFrom, EditorGUILayout.Vector3Field("Render To", hg.renderTo));
			}
			
			hg.renderMaterial = (Material) EditorGUILayout.ObjectField("Render Material", hg.renderMaterial, typeof(Material), false);
			hg.renderLineWidth = EditorGUILayout.IntField("Rendered Line Width", hg.renderLineWidth);
			
			hg.hideGrid = EditorGUILayout.Toggle("Hide Drawing", hg.hideGrid);
			hg.hideOnPlay = EditorGUILayout.Toggle("Hide While playing", hg.hideOnPlay);
			++EditorGUI.indentLevel;
			GUILayout.Label("Hide Axis (Render & Draw)");
			hg.hideAxis.x = EditorGUILayout.Toggle("X", hg.hideAxis.x);
			hg.hideAxis.y = EditorGUILayout.Toggle("Y", hg.hideAxis.y);
			hg.hideAxis.z = EditorGUILayout.Toggle("Z", hg.hideAxis.z);
			--EditorGUI.indentLevel;
			
			hg.drawOrigin = EditorGUILayout.Toggle("Draw Origin", hg.drawOrigin);
		}
		--EditorGUI.indentLevel;
		
		if (GUI.changed)
			EditorUtility.SetDirty (target);
	}
}
