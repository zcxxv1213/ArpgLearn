using UnityEngine;
using System.Collections;

/*
	ABOUT THIS SCRIPT
	
This script is not a MonoBehaviour, it's a public static class that doesn't inherit from
anything. The class has three functions: it stores a two-dimensional array of bool values
(I will call this array matrix from now on and its entries squares), it sets and returns
the value of squares based on which world positions they correspond to.
	Each square represents a face of a rectangular grid on the XY plane and its position in
the matrix mirrors its position in the grid with (0,0) being the lower left face, the first
index denotes the row and the second index the column of a face in the grid.
	This class works together with two other scripts, SetupForbiddenFiles and BlockSquare.
The matrix is built first by SetupForbiddenFiles in the Awake() function to make sure it is
built before any Start() function gets called. It just calls the matrix-building method and
passes which grid to use fore reference. Then each obstacle fires the Start() function from
its BlockSquare script to set their tiles as forbidden. The player's movement script makes use
of the CheckSquare function to find out if a face is forbidden or not, but it doesn't write
anything to the matrix.

This script demonstrates how you can use Grid Framework to store information about individual
tiles apart from the objects they belong to in a format accessible to all objects in the scene.
	The information does not have to be limited to just boolean values, you can extend this
approach to store any other information you might have and build more complex game machanics
around it.

NOTE: the purpose of the "originSquare" variable might not be clear at first. In matrix coordinates
the lower left square has (0,0) and all the other square are relative to it. However, if your grid
does not start at the origin the matrix coordinates and the grid coordinates don't match, i.e. if
your grid starts in (-1,2) then a square that's 3 unitsto the right and one up has grid coordinates
(2,3) and matrix coordinates (3,1). In order to properly convert between matrix and grid we need to
be able to tell the square's position relative to the lower left square. if your grid starts at (0,0)
this would all be irrelevant
*/

public static class ForbiddenTilesExample{
	public static bool[,] allowedTiles; //two-dimensional array of bool values
	public static GFRectGrid movementGrid; //the grid everything is based on
	public static int[] originSquare; //the grid coordinates of the lower left square used for reference (X and Y only)
	
	//builds the matrix and sets everything up, gets called by a script attached to the grid object
	public static void Initialize(GFRectGrid theGrid){
		movementGrid = theGrid;
		BuildMatrix(); //builds a default matrix that has all entries set to tru
		SetOriginSquare(); //stores the X and Y grid coordinates of the lower left square
	}
	
	//takes the grids size or rendering range and builds a magtrix based on that. All entries are set to true
	public static void BuildMatrix(){
		//amount of rows and columns, either based on size or rendering range (first entry rows, second one columns)
		int[] size = SetMatrixSize();
						
		//build a default matrix
		allowedTiles = new bool[size[0], size[1]];
		//set all entries to true
		for(int i = 0; i < size[0]; i++){
			for( int j = 0; j < size[1]; j++){
				allowedTiles[i,j] = true;
			}
		}
	}
	
	// how large should the matrix be? Depends on whether we use "size" or "custom rendering range"
	private static int[] SetMatrixSize(){
		int[] size = new int[2];
        size[0]= movementGrid.useCustomRenderRange ? Mathf.FloorToInt(Mathf.Abs(movementGrid.renderFrom[0] - movementGrid.renderTo[0]) / movementGrid.spacing[0]) :
            2 * Mathf.FloorToInt(movementGrid.size[0] / movementGrid.spacing[0]);
        size[1] = movementGrid.useCustomRenderRange ? Mathf.FloorToInt(Mathf.Abs(movementGrid.renderFrom[2] - movementGrid.renderTo[2]) / movementGrid.spacing[2]) :
            2 * Mathf.FloorToInt(movementGrid.size[2] / movementGrid.spacing[2]);
        //  for (int i = 0; i < 2; i++){
        //get the distance between both ends (in world units), divide it by the spacing (to get grid units) and round down to the nearest integer
        //	size[i] = movementGrid.useCustomRenderRange? Mathf.FloorToInt(Mathf.Abs(movementGrid.renderFrom[i] - movementGrid.renderTo[i]) / movementGrid.spacing[i]):
        //	2 * Mathf.FloorToInt(movementGrid.size[i] / movementGrid.spacing[i]);
        //}
        return size;
	}
	
	//stores the grid coodinates of the lower left square. This is needed to properly map a grid's face to a matrix entry. First we find out its world coordinates,
	// then we translate then to grid cordinates.
	public static void SetOriginSquare(){
		//get the grid coordinates of the box (see GetBoxCoordinates in documentation); we get three coordinates, but we only use X and Y
		//we add 0.1f * Vector3.one to avoid unexpected behaviour for edge cases dues to rounding and float (in)accuracy
		Vector3 box = movementGrid.useCustomRenderRange? movementGrid.GetBoxCoordinates(movementGrid.transform.position + movementGrid.renderFrom + 0.1f * Vector3.one):
			movementGrid.GetBoxCoordinates(movementGrid.transform.position - movementGrid.size + 0.1f * Vector3.one);
		originSquare = new int[2]{Mathf.RoundToInt(box.x), Mathf.RoundToInt(box.z)};
	}
	
	//takes world coodinates, finds the corresponding square and sets that entry to either true or false. Use it to disable or enable squares
	public static void RegisterSquare(Vector3 vec, bool status){
		//first find the square that belongs to that world position
		int[] square = GetSquare(vec);
        //then set its value
		allowedTiles[square[0],square[1]] = status;
	}
	
	//takes world coodinates, finds the corresponding square and returns the value of that square. Use it to cheack if a square is forbidden or not
	public static bool CheckSquare(Vector3 vec){
		int[] square = GetSquare(vec);
		return allowedTiles[square[0],square[1]];
	}
	
	//takes world coodinates and finds the corresponding square. The result is returned as an int array that contains that square's position in the matrix
	private static int[] GetSquare(Vector3 vec){
		int[] square = new int [2];
        square[0]= Mathf.RoundToInt(movementGrid.GetBoxCoordinates(vec)[0]) - originSquare[0];
        square[1] = Mathf.RoundToInt(movementGrid.GetBoxCoordinates(vec)[2]) - originSquare[1];
        // for (int i = 0; i < 2; i++){
        //	square[i] = Mathf.RoundToInt(movementGrid.GetBoxCoordinates(vec)[i]) - originSquare[i];
        //     Debug.Log(square[i]);
        //}
        return square;
	}
	
	// this returns the matrix as a string so you can read it yourself, like in a GUI for debugging (nothing grid-related going on here, feel free to ignore it)
	public static string MatrixToString(){
		string text = "Occupied fields are 1, free fields are 0:\n\n";
		for(int j = allowedTiles.GetLength(1)-1; j >= 0; j--){
			for(int i = 0; i < allowedTiles.GetLength(0); i++){
				text = text + (allowedTiles[i,j] ? "0" : "1") + " ";
			}
			text += "\n";
		}
		return text;
	}
}
