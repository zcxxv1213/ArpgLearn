using UnityEngine;
using System.Collections;

public class DragBlock : MonoBehaviour {
	
	public bool beingDragged;
	private Vector3 touchOffset; // the offset between the touch point and the centre
	private Vector3 lastSnap; // the last known save position we were able to snap to (the foresight works only from snap to snap)
	private Vector3[] bounds; // we can only move the block within these bounds (the grid itself is the largest possible bound)

	void Start () {
		beingDragged = false;
		SlidingPuzzleExample.RegisterObstacle(transform, false); //register this block in the matrix as occupied space
	}
	
	void OnMouseDown(){
		beingDragged = true; // start dragging
		touchOffset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position; //offset between the cursor and the block centre
		lastSnap = transform.position; // this is obviously where we snapped the last time
		bounds = SlidingPuzzleExample.CalculateSlidingBounds(transform.position, transform.lossyScale); // create default bounds
		SlidingPuzzleExample.RegisterObstacle(transform, true); // marks this space as free in the matrix (or else we won't be able to return back here)
	}
	
	void OnMouseUp(){
		beingDragged = false; // stop dragging
		SlidingPuzzleExample.mainGrid.AlignTransform(transform); // snap into position precisely
		transform.position = ClampPosition(transform.position); // clamp the position to be safe (because of possible rounding errors above)
		lastSnap = transform.position; //this is out last save position
		SlidingPuzzleExample.RegisterObstacle(transform, false); // mark the space as occupied again
	}
	
	void FixedUpdate () {
		if(beingDragged)
			Drag();
	}
	
	//this is where the dragging logic takes places
	void Drag(){
		// the destination is where the cursor points (minus the offset) clamped by the bounds
		Vector3 destination = ClampPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition) - touchOffset);
		destination.z = transform.position.z;
		
		// now use that information to get the new bounds
		bounds = SlidingPuzzleExample.CalculateSlidingBounds(lastSnap, transform.lossyScale);
		
		// simulate a snap to the grid so we can get potentially new bounds in the next step
		lastSnap = ClampPosition(SlidingPuzzleExample.mainGrid.AlignVector3(destination, transform.lossyScale));
		
		// move to the destination!
		transform.position = destination;
	}
	
	// don't let the block move out of the grid (uses the grid's renderFrom and renderTo)
	Vector3 ClampPosition(Vector3 vec){
		// if there are no other blocks in the way then at least the grid's RenderFrom and RenderTo must serve as bounds or else we go off grid
		Vector3 lowerLimit = Vector3.Max(SlidingPuzzleExample.mainGrid.GridToWorld(SlidingPuzzleExample.mainGrid.renderFrom) + 0.5f * transform.lossyScale, bounds[0]);
		Vector3 upperLimit = Vector3.Min(SlidingPuzzleExample.mainGrid.GridToWorld(SlidingPuzzleExample.mainGrid.renderTo) - 0.5f * transform.lossyScale, bounds[1]);
		
		upperLimit.z = transform.position.z; lowerLimit.z = transform.position.z;
		
		// this method of using the maximum of the minimum is similar to Unity's Mathf.Clamp(), except it is for vectors
		return Vector3.Max(lowerLimit, Vector3.Min(upperLimit, vec));
	}
}