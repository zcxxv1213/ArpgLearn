using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;


/* THINGS NEEDED

- being able to ignore certain kinds of objects (like cameras)
*/ 

public class GFGridAlignPanel : EditorWindow {
	GFGrid grid;
	Transform gridTransform;
	bool ignoreRootObjects;
	LayerMask affectedLayers;
	bool inculdeChildren;
	bool autoSnapping;
	GFBoolVector3 lockAxes = new GFBoolVector3(false);
	
	[MenuItem("Window/Grid Align Panel")]
	public static void Init(){
		GetWindow(typeof(GFGridAlignPanel), false, "Grid Align Panel");	
	}
	
	void OnGUI(){
		grid = (GFGrid) EditorGUILayout.ObjectField("Grid:", grid, typeof(GFGrid), true);
		if(GUI.changed && grid){
			if(grid){
				gridTransform = grid.transform;
			} else{
				gridTransform = null;
			}
		}
		ignoreRootObjects = EditorGUILayout.Toggle("Ignore Root Objects", ignoreRootObjects);
		affectedLayers = LayerMaskField("Affected Layers", affectedLayers);
		
		inculdeChildren = EditorGUILayout.Toggle("Include Children", inculdeChildren);
		
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Align Scene")){
			AlignScene();
		}
		if(GUILayout.Button("Align Selected")){
			AlignSelected();
		}
		EditorGUILayout.EndHorizontal();

		
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Scale Scene")){
			ScaleScene();
		}
		if(GUILayout.Button("Scale Selected")){
			ScalenSelected();
		}
		EditorGUILayout.EndHorizontal();
		
		autoSnapping = EditorGUILayout.Toggle("Auto-Snapping", autoSnapping);	
		
		GUILayout.Label("Lock axes for Aligning");
		++EditorGUI.indentLevel;
		lockAxes[0] = EditorGUILayout.Toggle("X", lockAxes[0]);
		lockAxes[1] = EditorGUILayout.Toggle("Y", lockAxes[1]);
		lockAxes[2] = EditorGUILayout.Toggle("Z", lockAxes[2]);
	}
	
	void Update(){
		if(!grid)
			return;
		
		if(autoSnapping && Selection.transforms.Length > 0){
			foreach(Transform selectedTransform in Selection.transforms){
				if(selectedTransform != gridTransform){
					if(selectedTransform)
						if(!AlreadyAligned(selectedTransform))
							grid.AlignTransform(selectedTransform, true, lockAxes, true);
				}
			}
		}
	}
	
	
	#region align
	
	void AlignScene(){
		if(!grid)
			return;
		
		List<Transform> allTransforms = new List<Transform>((Transform[])Transform.FindObjectsOfType(typeof(Transform)));
		RemoveAligned(ref allTransforms);
		
		if(allTransforms.Count == 0)
			return;
		
		Undo.RegisterSceneUndo("Align Scene");
		
		foreach(Transform curTransform in allTransforms){
			if(!(ignoreRootObjects && curTransform.parent == null && curTransform.childCount > 0) && (affectedLayers.value & 1<<curTransform.gameObject.layer) != 0){
//				Debug.Log(curTransform);
//				Debug.Log(affectedLayers.value & 1<<curTransform.gameObject.layer);
				grid.AlignTransform(curTransform, true, lockAxes, true);
				if(inculdeChildren){
					foreach(Transform child in curTransform){
						grid.AlignTransform(child, true, lockAxes, true);
					}
				}
			}
		}
	}
	
	void AlignSelected(){
		if(!grid)
			return;
		
		List<Transform> allTransforms = new List<Transform>((Transform[])Selection.transforms);
		RemoveAligned(ref allTransforms);
		
		if(allTransforms.Count == 0)
			return;
		
		Undo.RegisterSceneUndo("Align Selected");
		
		foreach(Transform curTransform in allTransforms){
			if(!(ignoreRootObjects && curTransform.parent == null && curTransform.childCount > 0)){
//				Debug.Log(curTransform);
				grid.AlignTransform(curTransform, true, lockAxes, true);
			}
			if(inculdeChildren){
				foreach(Transform child in curTransform){
					grid.AlignTransform(child, true, lockAxes, true);
				}
			}
		}
	}
	
	private bool AlreadyAligned(Transform trans){
		return (trans.position - grid.AlignVector3(trans.position, trans.lossyScale)).sqrMagnitude < 0.0001;
	}
	
	private void RemoveAligned(ref List<Transform> transformList){
		//we'll keep a counter for the amount of objects in the list to avoid calling transformList.Count each iteration
		int counter = transformList.Count;
		for(int i = 0; i <= counter - 1; i++){
			if(AlreadyAligned(transformList[i])){
				transformList.RemoveAt(i);
				i --; //reduce the indexer because we removed an entry from list
				counter --; //reduce the counter since the list has become smaller
			}
		}
		
		transformList.Remove(grid.transform);
	}
	
	#endregion
	
	#region scale
	
	void ScaleScene(){
		if(!grid)
			return;
		
		List<Transform> allTransforms = new List<Transform>((Transform[])Transform.FindObjectsOfType(typeof(Transform)));
		
		allTransforms.Remove(grid.transform);
		
		Undo.RegisterSceneUndo("Align Scene");
		
		foreach(Transform curTransform in allTransforms){
			if(!(ignoreRootObjects && curTransform.parent == null && curTransform.childCount > 0) && (affectedLayers.value & 1<<curTransform.gameObject.layer) != 0){
//				Debug.Log(curTransform);
				grid.ScaleTransform(curTransform, lockAxes);
			}
			if(inculdeChildren){
				foreach(Transform child in curTransform){
					grid.ScaleTransform(child, lockAxes);
				}
			}
		}
	}
	
	void ScalenSelected(){
		if(!grid)
			return;
		Transform gridTransform = grid.transform;
		
		Undo.RegisterSceneUndo("Align Scene");
		
		foreach(Transform curTransform in Selection.transforms){
			if(curTransform != gridTransform && !(ignoreRootObjects && curTransform.parent == null && curTransform.childCount > 0)){
//				Debug.Log(curTransform);
				grid.ScaleTransform(curTransform, lockAxes);
			}
			if(inculdeChildren){
				foreach(Transform child in curTransform){
					grid.ScaleTransform(child, lockAxes);
				}
			}
		}
	}
	
	#endregion
	
	#region LayerMask
	
	public static LayerMask LayerMaskField (string label, LayerMask selected) {
    	return LayerMaskField (label,selected,true);
	}
	
	
	public static LayerMask LayerMaskField (string label, LayerMask selected, bool showSpecial) {

	    List<string> layers = new List<string>();
		List<int> layerNumbers = new List<int>();

		string selectedLayers = "";

		for (int i=0;i<32;i++) {
			string layerName = LayerMask.LayerToName (i);

			if (layerName != "") {
				if (selected == (selected | (1 << i))) {
					if (selectedLayers == "") {
						selectedLayers = layerName;
					} else {
						selectedLayers = "Mixed";
					}
				}
			}
		}

//		EventType lastEvent = Event.current.type; //used for debugging

		if (Event.current.type != EventType.MouseDown && Event.current.type != EventType.ExecuteCommand) {
			if (selected.value == 0) {
				layers.Add ("Nothing");
			} else if (selected.value == -1) {
				layers.Add ("Everything");
			} else {
				layers.Add (selectedLayers);
			}
			layerNumbers.Add (-1);
		}

		if (showSpecial) {
			layers.Add ((selected.value == 0 ? "[X] " : "     ") + "Nothing");
			layerNumbers.Add (-2);

			layers.Add ((selected.value == -1 ? "[X] " : "     ") + "Everything");
			layerNumbers.Add (-3);
		}

		for (int i=0;i<32;i++) {

			string layerName = LayerMask.LayerToName (i);

			if (layerName != "") {
				if (selected == (selected | (1 << i))) {
					layers.Add ("[X] "+layerName);
				} else {
					layers.Add ("     "+layerName);
				}
				layerNumbers.Add (i);
			}
		}

		bool preChange = GUI.changed;

		GUI.changed = false;

		int newSelected = 0;

		if (Event.current.type == EventType.MouseDown) {
			newSelected = -1;
		}

		newSelected = EditorGUILayout.Popup (label,newSelected,layers.ToArray(),EditorStyles.layerMaskField);

		if (GUI.changed && newSelected >= 0) {
			//newSelected -= 1;

//			Debug.Log (lastEvent +" "+newSelected + " "+layerNumbers[newSelected]);

			if (showSpecial && newSelected == 0) {
				selected = 0;
			} else if (showSpecial && newSelected == 1) {
				selected = -1;
			} else {

				if (selected == (selected | (1 << layerNumbers[newSelected]))) {
					selected &= ~(1 << layerNumbers[newSelected]);
					//Debug.Log ("Set Layer "+LayerMask.LayerToName (LayerNumbers[newSelected]) + " To False "+selected.value);
				} else {
					//Debug.Log ("Set Layer "+LayerMask.LayerToName (LayerNumbers[newSelected]) + " To True "+selected.value);
					selected = selected | (1 << layerNumbers[newSelected]);
				}
			}
		} else {
			GUI.changed = preChange;
		}
	
//	Debug.Log(selected.value);
	return selected;
	}
	
	#endregion
}