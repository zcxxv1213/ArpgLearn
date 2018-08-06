using UnityEngine;
using System.Collections;

/*
	ABOUT THIS SCRIPT
	
This script is similar to the RoamGrid script from the grid-based movement example.
In addition this script will check before each move whether the target tile is
forbidden or not and if it is it will pick another dircetion. The information which
tiles are forbidden and which aren't is stored in a public static bool matrix.

This script demonstrates how you can use Grid Framework to store information about
individual tiles apart from the objects they belong to in a format accessible to all
objects in the scene.
*/

public class RoamGridWithWalls : MonoBehaviour {

	private GFRectGrid grid;
	public float roamingTime = 1.0f; //how long it takes to move from one tile to another
	
	//whether the object is to move or not, to where and how fast
	private bool doMove = false;
	private Vector3 goal;
	private float roamingSpeed;
	
	//cache the transform for performance
	private Transform cachedTransform;

	void Start(){
		cachedTransform = transform;
		grid = ForbiddenTilesExample.movementGrid;
	
		//make a check to prevent getting stuck in a null exception
		if(grid){
			//snap to the grid  no matter where we are
			grid.AlignTransform(cachedTransform);
		}
	}
	
	void Update(){
		if(!grid)
			return;
		
		if(doMove){
			//move towards the desination
			Vector3 newPosition = cachedTransform.position;
			newPosition.x = Mathf.MoveTowards(cachedTransform.position.x, goal.x, roamingSpeed * Time.deltaTime);
			newPosition.y = Mathf.MoveTowards(cachedTransform.position.y, goal.y, roamingSpeed * Time.deltaTime);
			cachedTransform.position = newPosition;
			//check if we reached the destination (use a certain tolerance so we don't miss the point becase of rounding errors)
			if(Mathf.Abs(cachedTransform.position.x - goal.x) < 0.01f && Mathf.Abs(cachedTransform.position.y - goal.y) < 0.01f)
				doMove = false;
			//if we did stop moving
		} else{
			//make sure the time is always positive
			if(roamingTime < 0.01f)
				roamingTime = 0.01f;
			//find the next destination
			goal = FindNextFace();
			//--- let's check if the goal is allowed, if not we will pick another direction during the next frame ---
			if(ForbiddenTilesExample.CheckSquare(goal)){
				//calculate speed by dividing distance (one of the two distances will be 0, we need the other one) through time
				roamingSpeed = Mathf.Max(Mathf.Abs(cachedTransform.position.x - goal.x), Mathf.Abs(cachedTransform.position.y - goal.y)) / roamingTime;
				//resume movement with the new goal
				doMove = true;
			} else{
				Debug.Log("hit the obstacle");
			}
		}
	}

	Vector3 FindNextFace(){
		//we will be operating in grid space, so convert the position
		Vector3 newPosition = grid.WorldToGrid(cachedTransform.position);
		
		//first let's pick a random number for one of the four possible directions
		int i = Random.Range(0, 4);
		//now add one grid unit onto position in the picked direction
		if(i == 0){
			newPosition = newPosition + new Vector3(1,0,0);
		} else if(i == 1){
			newPosition = newPosition + new Vector3(-1,0,0);
		} else if(i == 2){
			newPosition = newPosition + new Vector3(0,1,0);
		} else if(i == 3){
			newPosition = newPosition + new Vector3(0,-1,0);
		}
		//if we would wander off beyond the size of the grid turn the other way around
		for(int j = 0; j < 2; j++){
			if(Mathf.Abs(newPosition[j]) > grid.size[j] / grid.spacing[j])
				newPosition[j] -= Mathf.Sign(newPosition[j]) * 2.0f;
		}
		
		//return the position in world space
		return grid.GridToWorld(newPosition);
	}
}
