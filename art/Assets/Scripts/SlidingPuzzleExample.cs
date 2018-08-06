using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/*	ABOUT THIS SCRIPT

 For the sake of simplicity we will assume that the grid is always using a
 custom rendering range and starts at (0,0,0). If you want to see how to handle
 any grid please refer to the "movement with obstacles" example.
 
*/

public static class SlidingPuzzleExample{
    public static int[] originSquare; //the grid coordinates of the lower left square used for reference (X and Y only)
    private static GFRectGrid _grid; //private member variable for the grid
    

    public static GFRectGrid mainGrid{ // a public accessor for the grid
		get{return _grid;}
		set{_grid = value;
			BuildLevelMatrix(); //when we set a new grid we also immediately build a new matrix for it
            SetOriginSquare();

        }
	}
	
	// this is where we store our information; all game logic comes from this matrix
	private static int[,] levelMatrix;
		
	//takes the grid's rendering range and builds a matrix based on that. All entries are set to true
	private static void BuildLevelMatrix(){
		//amount of rows and columns, either based on size or rendering range (first entry rows, second one columns)
		int[] size = SetMatrixSize();

       LayerMask WalkableMask = (1 << 10);
        LayerMask UnWalkableMask = (1 << 9);
        //build a default matrix
        levelMatrix = new int[size[0], size[1]];
		//set all entries to true

        //默认都不可行走
		for(int i = 0; i < size[0]; i++){
			for( int j = 0; j < size[1]; j++){
				levelMatrix[i,j] = 2; // all squares allowed initially
			}
		}

        Vector3 worldBottomLeft = _grid.transform.position - Vector3.right * _grid.size.x - Vector3.forward * _grid.size.z;

        //for (int x = 0; x < size[0]; x++)
        //{
        //    for (int y = 0; y < size[1]; y++)
        //    {
        //        Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _grid.spacing.x + _grid.spacing.x / 2) + Vector3.forward * (y * _grid.spacing.z + _grid.spacing.z / 2);
        //        Ray ray = new Ray(worldPoint + Vector3.up * 30, Vector3.down);
        //        RaycastHit hit;
        //        //if (Physics.CheckSphere(worldPoint, _grid.spacing.x / 4, WalkableMask))
        //        //{
        //        //    Debug.Log("Walkable");
        //        //    levelMatrix[x, y] = 0;

        //        //}

        //        //if (Physics.SphereCast(ray, _grid.spacing.x / 4, out hit))
        //        //{
        //        //    if (hit.transform.gameObject.tag == "walkable")
        //        //        levelMatrix[x, y] = 0;
        //        //}


        //        Ray ray2 = new Ray(worldPoint + Vector3.left / 4 + Vector3.up * 30, Vector3.down);
        //        Ray ray3 = new Ray(worldPoint + Vector3.right / 4 + Vector3.up * 30, Vector3.down);
        //        Ray ray4 = new Ray(worldPoint + Vector3.forward / 4 + Vector3.up * 30, Vector3.down);
        //        Ray ray5 = new Ray(worldPoint + Vector3.back / 4 + Vector3.up * 30, Vector3.down);

        //        bool[] walk_able = new bool[4] { false, false, false, false };
      
        //        if (Physics.Raycast(ray2, out hit))
        //        {
        //            if (hit.transform.gameObject.tag == "walkable")
        //                walk_able[0] = true;
        //        }
        //        if (Physics.Raycast(ray3, out hit))
        //        {
        //            if (hit.transform.gameObject.tag == "walkable")
        //                walk_able[1] = true;
        //        }
        //        if (Physics.Raycast(ray4, out hit))
        //        {
        //            if (hit.transform.gameObject.tag == "walkable")
        //                walk_able[2] = true;
        //        }
        //        if (Physics.Raycast(ray5, out hit))
        //        {
        //            if (hit.transform.gameObject.tag == "walkable")
        //                walk_able[3] = true;

        //            // if (hit.transform.gameObject.tag == "wall")
        //            //    levelMatrix[x, y] = 1;
        //            //else if (hit.transform.gameObject.tag == "unwalkable")
        //            //    levelMatrix[x, y] = 2;
        //        }

        //        for (int i = 0; i < 4; i++ )
        //        {
        //            if (walk_able[i] == true)
        //            {
        //                levelMatrix[x, y] = 0;
        //            }
        //        }

        //    }
        //}

        //return;

        //:0可行走 1墙 2河流 3箱子 4草丛。

        for (int x = 0; x <size[0]; x++) {
            for (int y = 0; y < size[1]; y++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _grid.spacing.x+  _grid.spacing.x/2) + Vector3.forward * (y * _grid.spacing.z+ _grid.spacing.z / 2);
                
                Ray ray = new Ray(worldPoint+Vector3.up*30,Vector3.down);
                RaycastHit hit;

                //if (Physics.SphereCast(ray, _grid.spacing.x / 4, out hit))
                //{
                //    if (hit.transform.gameObject.tag == "wall")
                //        levelMatrix[x, y] = 1;
                //    else if (hit.transform.gameObject.tag == "unwalkable")
                //        levelMatrix[x, y] = 2;
                //}

                Ray ray2 = new Ray(worldPoint + Vector3.left / 4 + Vector3.up * 30, Vector3.down);
                Ray ray3 = new Ray(worldPoint + Vector3.right / 4 + Vector3.up * 30, Vector3.down);
                Ray ray4 = new Ray(worldPoint + Vector3.forward / 4 + Vector3.up * 30, Vector3.down);
                Ray ray5 = new Ray(worldPoint + Vector3.back / 4 + Vector3.up * 30, Vector3.down);

                bool[] walls = new bool[4]{ false, false, false, false };
                bool[] un_walks = new bool[4] { false, false, false, false };
                bool[] grass = new bool[4] { false, false, false, false };


                //只有四个角都是草丛才算草丛
                //只有四个角都有碰撞后端才不能走
                if (Physics.Raycast(ray2, out hit))
                {
                    if (hit.transform.gameObject.tag == "wall")
                        walls[0] = true;
                    else if (hit.transform.gameObject.tag == "unwalkable")
                        un_walks[0] = true;
                    else if (hit.transform.gameObject.tag == "grass")
                        grass[0] = true;
                }
                if (Physics.Raycast(ray3, out hit))
                {
                    if (hit.transform.gameObject.tag == "wall")
                        walls[1] = true;
                    else if (hit.transform.gameObject.tag == "unwalkable")
                        un_walks[1] = true;
                    else if (hit.transform.gameObject.tag == "grass")
                        grass[1] = true;
                }
                if (Physics.Raycast(ray4, out hit))
                {
                    if (hit.transform.gameObject.tag == "wall")
                        walls[2] = true;
                    else if (hit.transform.gameObject.tag == "unwalkable")
                        un_walks[2] = true;
                    else if (hit.transform.gameObject.tag == "grass")
                        grass[2] = true;
                }
                if (Physics.Raycast(ray5, out hit))
                {
                    if (hit.transform.gameObject.tag == "wall")
                        walls[3] = true;
                    else if (hit.transform.gameObject.tag == "unwalkable")
                        un_walks[3] = true;
                    else if (hit.transform.gameObject.tag == "grass")
                        grass[3] = true;

                    // if (hit.transform.gameObject.tag == "wall")
                    //    levelMatrix[x, y] = 1;
                    //else if (hit.transform.gameObject.tag == "unwalkable")
                    //    levelMatrix[x, y] = 2;
                }

                bool is_wall = false;
                bool un_walk = true;
                bool is_grass = true;


                for (int i = 0; i < 4; i++)
                {
                    if (grass[i] == false)
                    {
                        is_grass = false;
                        break;
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (walls[i] == true)
                    {
                        is_wall = true;
                        break;
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (un_walks[i] == false)
                    {
                        un_walk = false;
                        break;
                    }
                }

                //0可行走 1墙 2河流 3箱子 4草丛。
                if (is_grass == true)
                {
                    levelMatrix[x, y] = 4;
                }
                else if (is_wall == true)
                {
                    levelMatrix[x, y] = 1;
                }
                else if (un_walk == true)
                {
                    levelMatrix[x, y] = 2;
                }
                else
                {
                    levelMatrix[x, y] = 0;
                }
            }
        }
        
    }
	
	// how large should the matrix be? For the sake of simplicity we only use the rendering range here
	private static int[] SetMatrixSize(){
		int[] size = new int[2];
        // size[0] = Mathf.FloorToInt(_grid.renderTo[0] / _grid.spacing[0]);
        //size[1] = Mathf.FloorToInt(_grid.renderTo[2] / _grid.spacing[2]);
        size[0] = _grid.useCustomRenderRange ? Mathf.FloorToInt(Mathf.Abs(_grid.renderFrom[0] - _grid.renderTo[0]) / _grid.spacing[0]) :
            2 * Mathf.FloorToInt(_grid.size[0] / _grid.spacing[0]);
        size[1] = _grid.useCustomRenderRange ? Mathf.FloorToInt(Mathf.Abs(_grid.renderFrom[2] - _grid.renderTo[2]) / _grid.spacing[2]) :
            2 * Mathf.FloorToInt(_grid.size[2] / _grid.spacing[2]);
        for (int i = 0; i < 2; i++){
			//get the distance between both ends (in world units), divide it by the spacing (to get grid units) and round down to the nearest integer
			
		}
		return size;
	}
	
	//takes world coodinates, finds the corresponding square and sets that entry to either true or false. Use it to disable or enable squares
	public static void RegisterObstacle(Transform obstacle, bool state){
		//first break up the obstacle into several 1x1 obstacles
		Vector3[,] parts = BreakUpObstacle(obstacle);
		
		//now find the square of each part and set it to true or false
		//for(int i = 0; i < parts.GetLength(0); i++){
		//	for(int j = 0; j < parts.GetLength(1); j++){
		//		int[] square = GetSquare(parts[i,j]);
		//		levelMatrix[square[0],square[1]] = state;
		//	}
		//}
	}
    public static void SetOriginSquare()
    {
        //get the grid coordinates of the box (see GetBoxCoordinates in documentation); we get three coordinates, but we only use X and Y
        //we add 0.1f * Vector3.one to avoid unexpected behaviour for edge cases dues to rounding and float (in)accuracy
        Vector3 box = _grid.useCustomRenderRange ? _grid.GetBoxCoordinates(_grid.transform.position + _grid.renderFrom + 0.1f * Vector3.one) :
            _grid.GetBoxCoordinates(_grid.transform.position - _grid.size + 0.1f * Vector3.one);
        originSquare = new int[2] { Mathf.RoundToInt(box.x), Mathf.RoundToInt(box.z) };
    }
    // When we want to slide a block we need to know how far we can go before we "collide" (note that there is no actual collision detection involved anywhere).
    // We can only look up to on square ahead in each direction, so the bounds need to be recalculated from time to time; this allows us to have obstacles in
    // all sorts of directions, like a maze that can change all the time.
    public static Vector3[] CalculateSlidingBounds(Vector3 pos, Vector3 scl){
		//break up the block and find the lwer left and upper right square in the matrix
		Vector3[,] squares = BreakUpObstacle(pos, scl);
		int[] lowerLeft = GetSquare(squares[0,0]); // we store the position inside the matrix here
		int[] upperRight = GetSquare(squares[squares.GetLength(0)-1,squares.GetLength(2)-1]);
		//for each adjacent left square check if all left fields are free (a bitmask would have been the way to go instead of four bools, but let's keep it simple)
		bool freeLeft = true;
		//iterate over all the squares one square left of the left edge
		for( int i = lowerLeft[1]; i < upperRight[1] + 1; i++){
		//	freeLeft = freeLeft && levelMatrix[Mathf.Max(0, lowerLeft[0]-1),i]; // use the Max so we don't get negative values (the matrix starts at 0)
		}
		
		bool freeRight = true;
		//iterate
		for( int i = lowerLeft[1]; i < upperRight[1] + 1; i++){
	//		freeRight = freeRight && levelMatrix[Mathf.Min(levelMatrix.GetLength(0)-1, upperRight[0]+1),i]; // use Min so we don't go off the matrix size
		}
		
		bool freeBottom = true;
		for( int i = lowerLeft[0]; i < upperRight[0] + 1; i++){
	//		freeBottom = freeBottom && levelMatrix[i, Mathf.Max(0, lowerLeft[1]-1)];
		}
		
		bool freeTop = true;
		for( int i = lowerLeft[0]; i < upperRight[0] + 1; i++){
	//		freeTop = freeTop && levelMatrix[i, Mathf.Min(levelMatrix.GetLength(1)-1, upperRight[1]+1)];
		}
		
		//now assume the block canot move anywhere; for each free direction loosen the constraints by one grid unit (world unit * spacing)
		Vector3[] bounds = new Vector3[2] {pos, pos};
		if(freeLeft)
			bounds[0] -= _grid.spacing.x*Vector3.right;
		if(freeRight)
			bounds[1] += _grid.spacing.x*Vector3.right;
		if(freeBottom)
			bounds[0] -= _grid.spacing.z*Vector3.up;
		if(freeTop)
			bounds[1] += _grid.spacing.z*Vector3.up;
		
		// the bounds can still be outside of the grid, so we need to clamp that as well
		for(int i = 0; i < 2; i++){
			for(int j = 0; j < 2; j++){
				bounds[i][j] = Mathf.Clamp(bounds[i][j], _grid.GridToWorld(Vector3.zero)[j] + 0.5f * scl[j], _grid.GridToWorld(_grid.renderTo)[j] - 0.5f * scl[j]);
			}
		}
		
		return bounds;
	}
	
	// break a large obstacle spanning several squares into several obstacles spanning one square each
	public static Vector3[,] BreakUpObstacle(Vector3 pos, Vector3 scl){
		// first convert the scale to int and store X and Y values separate
		int[] obstacleScale = new int[2];
		for(int i = 0; i < 2; i++){
			
		}
        obstacleScale[0] = Mathf.Max(1, Mathf.RoundToInt(scl[0])); //no lower than 1
        obstacleScale[1] = Mathf.Max(1, Mathf.RoundToInt(scl[2])); //no lower than 1
            // we will apply a shift so we always get the centre of the broken up parts, the shift depends on whether even or odd
        Vector3[] shift = new Vector3[2];
		for(int k = 0; k < 2; k++){
            if (k != 1)
            {
                if (obstacleScale[0] % 2 == 0)
                {

                    shift[k] = (-obstacleScale[k] / 2.0f + 0.5f) * (k == 0 ? Vector3.right : Vector3.up);
                }
                else
                {
                    shift[k] = (-(obstacleScale[k] - 1) / 2.0f) * (k == 0 ? Vector3.right : Vector3.up);
                }
            }
            else
            {
                if (obstacleScale[0] % 2 == 0)
                {

                    shift[1] = (-obstacleScale[1] / 2.0f + 0.5f) * (new Vector3(0,0,1));
                }
                else
                {
                    shift[1] = (-(obstacleScale[1] - 1) / 2.0f) * (new Vector3(0, 0, 1));
                }
            }
        }
        
		// this is where we store the single obstacles
		Vector3[,] obstacleMatrix = new Vector3[obstacleScale[0],obstacleScale[1]];
		// now break the obstacle up into squares and handle each square individually like an obstacle
		for(int i = 0; i < obstacleScale[0]; i++){
			for(int j = 0; j < obstacleScale[1]; j++){
				obstacleMatrix[i,j] = pos + shift[0] + shift[1] + Vector3.Scale(new Vector3(i, 0,j), SlidingPuzzleExample.mainGrid.spacing); // <-- wrong!
			}
		}
		
		return obstacleMatrix;
	}
	
	// an alternative to the above that takes in a Transform as an argument
	public static Vector3[,] BreakUpObstacle(Transform obstacle){
		return BreakUpObstacle(obstacle.position, obstacle.lossyScale);
	}
	
	// take world coodinates and find the corresponding square. The result is returned as an int array that contains that square's position in the matrix
	private static int[] GetSquare(Vector3 vec){
		int[] square = new int [2];
        square[0] = Mathf.RoundToInt(_grid.GetBoxCoordinates(vec)[0]) - originSquare[0];
        square[1] = Mathf.RoundToInt(_grid.GetBoxCoordinates(vec)[2]) - originSquare[1];
        //square[0] = Mathf.RoundToInt(_grid.GetBoxCoordinates(vec)[0]);
       // square[1] = Mathf.RoundToInt(_grid.GetBoxCoordinates(vec)[2]);
        for (int i = 0; i < 2; i++){
			
		}
		return square;
	}

    // this returns the matrix as a string so you can read it yourself, like in a GUI for debugging (nothing grid-related going on here, feel free to ignore it)
    //public static string MatrixToString(){
    //       int[] size = SetMatrixSize();
    //       //string text = "Occupied fields are 1, free fields are 0:\n\n"+ "\r\n ";
    //       string text = "-module(data_map_" + SceneManager.GetActiveScene().name + ")." + "\r\n ";
    //       text = text + "-compile(export_all)." + "\r\n ";
    //       text = text + "get() -> {map_config," + SceneManager.GetActiveScene().name + "," + size[1] + "," + "\r\n ";
    //       for (int j = levelMatrix.GetLength(1)-1; j >= 0; j--){
    //		for(int i = 0; i < levelMatrix.GetLength(0); i++){
    //               if (i == levelMatrix.GetLength(0) - 1 && j == 0)
    //               {
    //                   text = text + (levelMatrix[i, j] ? "0" : "1") + "\">>}.";
    //               }
    //               else if (i ==0 && j == levelMatrix.GetLength(1) - 1)
    //               {
    //                   text = text + "<<\"" +(levelMatrix[i, j] ? "0" : "1");
    //               }
    //               else {
    //                   text = text + (levelMatrix[i, j] ? "0" : "1");
    //               }
    //		}
    //	}

    //       string returnString = text.Replace(" ", "");
    //	return returnString;
    //}
    public static string MatrixToString()
    {
        int[] size = SetMatrixSize();
        //string text = "Occupied fields are 1, free fields are 0:\n\n"+ "\r\n ";
        string text = "-module(data_map_" + SceneManager.GetActiveScene().name + ")." + "\r\n ";
        text = text + "-compile(export_all)." + "\r\n ";
        text = text + "get() -> {map_config," + SceneManager.GetActiveScene().name + "," + size[0] + "," + size[1] + "," + "\n ";
        for (int j = 0; j < levelMatrix.GetLength(1); j++)
        {
            for (int i = 0; i < levelMatrix.GetLength(0); i++)
            {
                if (i == levelMatrix.GetLength(0) - 1 && j == levelMatrix.GetLength(1)-1)
                {
                    text = text + (levelMatrix[i, j] ) + "\"}.";
                }
                else if (i == 0 && j == 0)
                {
                    text = text + "\"" + "\n" + (levelMatrix[i, j] );
                }
                else
                {
                    text = text + (levelMatrix[i, j] );
                }
            }
            text += "\n";
        }
        string returnString = text.Replace(" ", "");
        return returnString;
    }
    public static string BuildLua()
    {
        ResetGrass();
        int[] size = SetMatrixSize();
        string text = "local mLuaClass = require \"Core/LuaClass\"" + "\r\n ";
        text= text+ "local mBaseLua = require \"Core/BaseLua\"" + "\r\n ";
        text = text + "local " + "Map" + SceneManager.GetActiveScene().name+ "=" + "mLuaClass(\"" + "Map" + SceneManager.GetActiveScene().name +  "\",mBaseLua)" + "\r\n ";
        text = text + "local length = " + size[0] + "\r\n ";
        text = text + "local height = " + size[1] + "\r\n ";
        text = text + "local "  + "MapTable" + SceneManager.GetActiveScene().name + "={"+"\n";
        for (int j = 0; j < levelMatrix.GetLength(1); j++)
        {
            for (int i = 0; i < levelMatrix.GetLength(0); i++)
            {
                if (i == levelMatrix.GetLength(0) - 1 && j == levelMatrix.GetLength(1) - 1)
                {
                    text = text + (levelMatrix[i, j]) + "}" + "\n";
                }
                else
                text = text + (levelMatrix[i, j] ) + ",";
            }
            text += "\n";
        }
        text = text + "function " + "Map" + SceneManager.GetActiveScene().name +  ":LoadInfo()" + "\n";
        text = text + "    local Array={}" + "\n";
        text = text + "    self.height = height" + "\n"; 
        text = text + "    self.length = length" + "\n"; 
        text = text + "    for i = 1,height do" + "\n" + "        local temp = {}" + "\n" + "        for j = 1,length do" + "\n" + "            if i ~= 1 then" + "\n"
            + "                temp[j] = " + "MapTable" + SceneManager.GetActiveScene().name + "[j+(i-1)*length]" + "\n"
            + "            else" + "\n" + "                temp[j] = " + "MapTable" + SceneManager.GetActiveScene().name + "[j]" + "\n" + "            end" + "\n" 
            + "        end" + "\n"
            + "        Array[i] = temp" + "\n"
            + "    end" + "\n"
            + "    self.Array = Array"
            + "\n" + "end" + "\n" ;
        text = text+ "function " + "Map" + SceneManager.GetActiveScene().name  + ":Test(x,y)" + "\n";
        text = text + "    local height = self.height" + "\n"; ;
        text = text + "    local length = self.length" + "\n"; ;
        text = text + "    if x<=height and y<=length and x>0 and y>0 then " + "\n"
            + "        if self.Array[x][y] then" + "\n"
            + "            return self.Array[x][y]  " + "\n"
            + "        else" + "\n"
            + "             return 1  " + "\n"
            + "        end" + "\n"
            + "    else" + "\n"
            + "        return 1 " + "\n"
            + "    end " + "\n" 
            + "end"+"\n";
        text = text + "return " + "Map" + SceneManager.GetActiveScene().name;
        return text;
    }
    static int GrassID = 10;
    private static void ResetGrass()
    {
        for (int j = 1; j < levelMatrix.GetLength(1); j++)
        {
            for (int i = 1; i < levelMatrix.GetLength(0); i++)
            {
                if (levelMatrix[i, j] == 4)
                {
                    GrassID += 1;
                    RecursiveQuery(i, j);
                }
            }
        }
    }
    private static void RecursiveQuery(int x,int y)
    {
       // if (x == levelMatrix.GetLength(0))
       //     Debug.Log(x + "    " + y);
        levelMatrix[x, y] = GrassID;
        int minX, maxX, minY, maxY;
        if (x == 1)
            minX = 1;
        else
            minX = x - 1;
        if (x == levelMatrix.GetLength(0))
            maxX = x;
        else
            maxX = x + 1;
        if (y == 1)
            minY = 1;
        else
            minY = y - 1;
        if (y == levelMatrix.GetLength(1))
            maxY = y;
        else
            maxY = y + 1;

        int grassNums = GetGrassNum(x, y);

        if (grassNums > 0)
        {
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    if (levelMatrix[i, j] == 4 && (i != x || j != y))
                    {
                        RecursiveQuery(i, j); 
                    }
                }
            }
        }
    }

    private static int GetGrassNum(int x, int y)
    {
        int minX, maxX, minY, maxY;
        if (x == 1)
            minX = 1;
        else
            minX = x - 1;
        if (x == levelMatrix.GetLength(0))
            maxX = x;
        else
            maxX = x + 1;
        if (y == 1)
            minY = 1;
        else
            minY = y - 1;
        if (y == levelMatrix.GetLength(1))
            maxY = y;
        else
            maxY = y + 1;

        int num = 0;

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY;j++)
            {
                if ((levelMatrix[i, j] == 4 || GrassID == levelMatrix[i, j])&&(i != x || j != y) )
                {
                    num += 1;
                }
            }
        }
        return num;
    }

    public static string ShowMatrixToString()
    {
        string text = "Occupied fields are 1, free fields are 0:\n\n";
        for (int j = levelMatrix.GetLength(1) - 1; j >= 0; j--)
        {
            for (int i = 0; i < levelMatrix.GetLength(0); i++)
            {
                text = text + (levelMatrix[i, j] ) + " ";
            }
            text += "\n";
        }
        return text;
    }
    // This method was used at some point but now it is of no use; I left it in though for you if you are interested
    /*	//takes world coodinates, finds the corresponding square and returns the value of that square. Use it to cheack if a square is forbidden or not
        public static bool CheckObstacle(Transform obstacle){
            bool free = true; // assume it is allowed
            //first break up the obstacle into several 1x1 obstacles
            Vector3[,] parts = BreakUpObstacle(obstacle);
            //now find the square of each part and set it to true or false
            for(int i = 0; i < parts.GetUpperBound(0) + 1; i++){
                for(int j = 0; j < parts.GetUpperBound(1) + 1; j++){
                    int[] square = GetSquare(parts[i,j]);
                    free = free && levelMatrix[square[0],square[1]]; // add all the entries, returns true if and only if all are true
                }
            }
            return free;
        }
    */
}