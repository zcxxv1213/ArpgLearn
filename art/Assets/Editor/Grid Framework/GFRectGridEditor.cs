using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(GFRectGrid))]
public class GFRectGridEditor : Editor {
	
	private GFRectGrid rg;
	private bool showDrawSettings;
	
	void OnEnable(){
		rg = target as GFRectGrid;
		showDrawSettings = EditorPrefs.HasKey("GFRectGridShowDraw") ? EditorPrefs.GetBool("GFRectGridShowDraw") : true;
	}
	
	void OnDisable(){
		EditorPrefs.SetBool("GFRectGridShowDraw", showDrawSettings);
	}

	public override void OnInspectorGUI(){		
		rg.relativeSize = EditorGUILayout.Toggle("Relative Size", rg.relativeSize);
		rg.size = EditorGUILayout.Vector3Field("Size", rg.size);
		rg.spacing = EditorGUILayout.Vector3Field("Spacing", rg.spacing);
		
		EditorGUILayout.Space();
		
		GUILayout.Label("Axis Colors");
		
		EditorGUILayout.BeginHorizontal();
		++EditorGUI.indentLevel;
		rg.axisColors.x = EditorGUILayout.ColorField(rg.axisColors.x);
		rg.axisColors.y = EditorGUILayout.ColorField(rg.axisColors.y);
		rg.axisColors.z = EditorGUILayout.ColorField(rg.axisColors.z);
		--EditorGUI.indentLevel;
		EditorGUILayout.EndHorizontal();
		
		rg.useSeparateRenderColor = EditorGUILayout.Foldout(rg.useSeparateRenderColor, "Use Separate Render Color");
		if(rg.useSeparateRenderColor){
			EditorGUILayout.BeginHorizontal();
			++EditorGUI.indentLevel;
			rg.renderAxisColors.x = EditorGUILayout.ColorField(rg.renderAxisColors.x);
			rg.renderAxisColors.y = EditorGUILayout.ColorField(rg.renderAxisColors.y);
			rg.renderAxisColors.z = EditorGUILayout.ColorField(rg.renderAxisColors.z);
			--EditorGUI.indentLevel;
		EditorGUILayout.EndHorizontal();
		}
		
		rg.vertexColor = EditorGUILayout.ColorField("Vertex Color", rg.vertexColor);
		
		EditorGUILayout.Space();
		
		showDrawSettings = EditorGUILayout.Foldout(showDrawSettings, "Draw & Render Settings");
		++EditorGUI.indentLevel;
		if(showDrawSettings){
			rg.renderGrid = EditorGUILayout.Toggle("Render Grid", rg.renderGrid);
			
			rg.useCustomRenderRange = EditorGUILayout.Foldout(rg.useCustomRenderRange, "Custom Rendering Range");
			if(rg.useCustomRenderRange){
				rg.renderFrom = Vector3.Min(rg.renderTo, EditorGUILayout.Vector3Field("Render From", rg.renderFrom));
				rg.renderTo = Vector3.Max(rg.renderFrom, EditorGUILayout.Vector3Field("Render To", rg.renderTo));
			}
			
			rg.renderMaterial = (Material) EditorGUILayout.ObjectField("Render Material", rg.renderMaterial, typeof(Material), false);
			rg.renderLineWidth = EditorGUILayout.IntField("Rendered Line Width", rg.renderLineWidth);
			
			rg.hideGrid = EditorGUILayout.Toggle("Hide Drawing", rg.hideGrid);
			rg.hideOnPlay = EditorGUILayout.Toggle("Hide While playing", rg.hideOnPlay);
			++EditorGUI.indentLevel;
			GUILayout.Label("Hide Axis (Render & Draw)");
			rg.hideAxis.x = EditorGUILayout.Toggle("X", rg.hideAxis.x);
			rg.hideAxis.y = EditorGUILayout.Toggle("Y", rg.hideAxis.y);
			rg.hideAxis.z = EditorGUILayout.Toggle("Z", rg.hideAxis.z);
			--EditorGUI.indentLevel;
			
			rg.drawOrigin = EditorGUILayout.Toggle("Draw Origin", rg.drawOrigin);
		}
		--EditorGUI.indentLevel;
		
		if (GUI.changed)
			EditorUtility.SetDirty (target);
		
	}
}