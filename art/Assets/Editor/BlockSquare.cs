using UnityEngine;
using System.Collections;

public class BlockSquare : MonoBehaviour {

	// Start() is called after Awake(), this ensures that the matrix has alrady been built
	void Start () {
		//Set the entry that corresonds to the obstacle's position as false
		ForbiddenTilesExample.RegisterSquare(transform.position, false);
	}
}
