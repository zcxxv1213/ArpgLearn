using UnityEngine;
using System.Collections;
[RequireComponent(typeof (GFRectGrid))]

public class SetupForbiddenTiles : MonoBehaviour {

	// Awake is called before Start()
	void Awake () {
		//We will build the matrix based on the grid that is attached to this object.
		//All entries are true by default, then each obstacle will mark its entry as false
		ForbiddenTilesExample.Initialize(GetComponent<GFRectGrid>());
	}
	
	void OnGUI(){
		GUI.TextArea (new Rect (200, 200, 500, 500), ForbiddenTilesExample.MatrixToString());
	}
}