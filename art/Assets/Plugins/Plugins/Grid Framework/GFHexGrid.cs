using UnityEngine;
using System.Collections;

public class GFHexGrid : GFGrid {
	
	[SerializeField]
	private float _radius = 1.0f;
	public float radius{
		get{return _radius;}
		set{if(value == _radius)// needed because the editor fires the setter even if this wasn't changed
				return;
			_radius = Mathf.Max(value, 0.1f);
			_gridChanged = true;
		}
	}
	
	[SerializeField]
	private float _depth = 1.0f;
	public float depth{
		get{return _depth;}
		set{if(value == _depth)// needed because the editor fires the setter even if this wasn't changed
				return;
			_depth = Mathf.Max(value, 0.1f);
			_gridChanged = true;
		}
	}
	
	#region helper values (read only)
	//use these helper values to keep the formulae simple (everything depends on radius)
	private float side{get{return 1.5f * radius;}}
	private float height{get{return Mathf.Sqrt(3.0f) * radius;}}	
	
	// the indices of the axes transformed to quasi-spcae (i.e. the Z-axis works like the Y-axis in XZ-grids)
	private int[] idx{get{return TransformIndices(gridPlane);}}
	
	// the normal X-, Y- and Z- vectors in world-space
	protected Vector3[] units{get{return new Vector3[]{Vector3.right, Vector3.up, Vector3.forward};}}
	
	//right, up and forward relative to the grid's Transform (i.e. in local space)
	private Vector3[] unitVectors{get{return new Vector3[3]{_transform.right, _transform.up, _transform.forward};}}
	private Vector2 slopedUnit{get{return hexSideMode == HexOrientation.PointySides ? new Vector2(1.5f * radius, 0.5f * height) : new Vector2(0.5f * height, 1.5f * radius);}}
	#endregion
	
	// the hexes will be parallel the the specified plane
	[SerializeField]
	protected GridPlane _gridPlane = GridPlane.XY;
	public GridPlane gridPlane {get{return _gridPlane;}set{if(value == _gridPlane){return;} _gridPlane = value; _gridChanged = true;}}
	
	// there are two ways a hexagon can be rotated; "sides" is parallel to the grid's Y-axis
	public enum HexOrientation {PointySides, FlatSides};
	[SerializeField]
	protected HexOrientation _hexSideMode = HexOrientation.PointySides;
	public HexOrientation hexSideMode {get{return _hexSideMode;}set{if(value == _hexSideMode){return;} _hexSideMode = value; _gridChanged = true;}}
		
	//different coordinate systems for hexagonal grids
	protected enum HexCoordinateSystem {Herring, SlopedCartesian, Flattened3D};
	
	// different shapes of hexagonal grids
	//public enum HexGridShape {Rectangle, CompactRectangle, Rhomb, BigHex, Triangle}
	public enum HexGridShape {Rectangle, CompactRectangle}
	[SerializeField]
	protected HexGridShape _gridStyle = HexGridShape.Rectangle;
	public HexGridShape gridStyle {get{return _gridStyle;}set{if(value == _gridStyle){return;} _gridStyle = value; _gridChanged = true;}}
	
	//the position of a vertex relative to the centre of a given hex
	protected enum HexVertex {N, NE, E, SE, S, SW, W, NW};
	
//_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
	
		
	#region grid to space
	
	public override Vector3 WorldToGrid(Vector3 worldPoint){
		Vector3 localPoint = _transform.InverseTransformDirection(worldPoint - FindNearestFace(_transform.position, gridPlane));
//		Debug.Log(localPoint);
//		Gizmos.DrawLine(worldPoint, FindNearestFace(_transform.position, gridPlane));
		
		int strip = Mathf.FloorToInt(localPoint[idx[0]] / (1.5f * radius));
		float remainder = (localPoint[idx[0]] - strip * 1.5f * radius) / (1.5f * radius);
		//float vert = (localPoint - remainder * (Vector3)slopedUnit)[idx[1]] / height;
		float vert = ((localPoint - remainder * (Vector3)(strip % 2 == 0 ? slopedUnit : slopedUnit.GFConjugate()))[idx[1]]
			- (strip % 2 == 0 ? 0 : 0.5f * height)) / height;
		
//		Debug.Log("strip: " + strip + ", remainder: " + remainder+ ", vert: " + vert);
		
		Vector3 gridPoint = (strip + remainder) * units[idx[0]] + vert * units[idx[1]] + localPoint[idx[2]] / depth * units[idx[2]];
		
		return gridPoint;
	}
	
	public override Vector3 GridToWorld(Vector3 gridPoint){
		int strip = Mathf.FloorToInt(gridPoint[idx[0]]);
		float remainder = gridPoint[idx[0]] - strip;
		Vector3 vec = strip % 2 == 0 ? slopedUnit : slopedUnit.GFConjugate(hexSideMode == HexOrientation.FlatSides);
		Vector3 unit = unitVectors[idx[0]] * vec[idx[0]] + unitVectors[idx[1]] * vec[idx[1]];
//		Debug.Log(unit);
				
		Vector3 worldPoint = strip * 1.5f * radius * unitVectors[idx[0]] + gridPoint[idx[1]] * height * unitVectors[idx[1]]
			+ gridPoint[idx[2]] * depth * unitVectors[idx[2]] + remainder * unit + (strip % 2 == 0 ? Vector3.zero : 0.5f * height * unitVectors[idx[1]]);

		worldPoint = worldPoint + FindNearestFace(_transform.position, gridPlane);
		
		return worldPoint;
	}
	
	#endregion
	
	
	#region finding functions
	#region Overloads
	public Vector3 FindNearestFace(Vector3 fromPoint, bool doDebug = false){
		return FindNearestFace(fromPoint, gridPlane, doDebug);
	}
	#endregion
	
	public override Vector3 FindNearestVertex(Vector3 fromPoint, bool doDebug = false){
		Vector3 face = FindNearestFace(fromPoint, gridPlane); //find the nearest face in the world
		// calculate the offset of the point from the centre of the hex (local 3D coordinates)
		Vector3 facePoint = _transform.GFInverseTransformPointFixed(fromPoint) - _transform.GFInverseTransformPointFixed(face); // Vector3 -> Vector2
				
		//inside the tile (hex) there are six possibilities, we need to find the right one
		HexVertex vert = GetHexVert(facePoint);
		
		Vector3 toPoint = face;// use the centre of the hex as a starting point
		
		// now move to the proper vertex
		if(vert == HexVertex.NE || vert == HexVertex.NW){// move up
			toPoint += unitVectors[idx[1]] * 0.5f * height;
		} else if(vert == HexVertex.SE || vert == HexVertex.SW){// move down
			toPoint -= unitVectors[idx[1]] * 0.5f * height;
		}
		
		if(vert == HexVertex.NE || vert == HexVertex.SE){// move right
			toPoint += unitVectors[idx[0]] * 0.5f * radius;
		} else if(vert == HexVertex.NW || vert == HexVertex.SW){// move left
			toPoint -= unitVectors[idx[0]] * 0.5f * radius;
		}
		
		if(vert == HexVertex.W){//move left
			toPoint -= unitVectors[idx[0]] * radius;
		} else if(vert == HexVertex.E){//move right
			toPoint += unitVectors[idx[0]] * radius;
		}
		
		if(doDebug){
			Gizmos.DrawSphere(toPoint, 0.3f);
		}
		return toPoint;
	}
	
	public override Vector3 FindNearestFace(Vector3 fromPoint, GridPlane thePlane, bool doDebug = false){
		Vector3 face = GetFaceCoordinates(fromPoint, thePlane);
		Vector3 toPoint = new Vector3();
		
		toPoint[idx[0]] = face[idx[0]] * side;
		toPoint[idx[1]] = face[idx[1]] * height + Mathf.Abs(face[idx[0]] % 2) * height / 2.0f;
		toPoint[idx[2]] = face[idx[2]] * depth;
		toPoint = _transform.GFTransformPointFixed(toPoint);
		
		if(doDebug){
			Gizmos.DrawSphere(toPoint, height / 2);
		}
		
		return toPoint;
	}
	
	public override Vector3 FindNearestBox(Vector3 fromPoint, bool doDebug = false){
		Vector3 box = GetBoxCoordinates(fromPoint);
		Vector3 toPoint = new Vector3();
		
		toPoint[idx[0]] = box[idx[0]] * side + radius;
		toPoint[idx[1]] = box[idx[1]] * height + Mathf.Abs(box[idx[0]] % 2) * height / 2.0f + 0.5f * height;
		toPoint[idx[2]] = box[idx[2]] * depth + 0.5f * depth;
		toPoint = _transform.GFTransformPointFixed(toPoint);
		
		if(doDebug){
			Gizmos.DrawSphere(toPoint, height / 2);
		}
		
		return toPoint;
	}
	
	#endregion

	
	#region get coordinates
	#region overloads
	public override Vector3 GetVertexCoordinates (Vector3 world)
	{
		return GetVertexCoordinates(world, HexCoordinateSystem.Herring);
	}
	public override Vector3 GetFaceCoordinates (Vector3 world, GridPlane thePlane)
	{
		return GetFaceCoordinates(world, thePlane, HexCoordinateSystem.Herring);
	}
	public override Vector3 GetBoxCoordinates (Vector3 world)
	{
		return GetBoxCoordinates(world, HexCoordinateSystem.Herring);
	}
	
	protected Vector3 GetVertexCoordinates (Vector3 world, HexCoordinateSystem coordianteSystem){
		if(coordianteSystem == HexCoordinateSystem.Herring){
			return GetVertexCoordinatesHerring(world);
		} else{
			return Vector3.zero;
		}
	}
	protected Vector3 GetFaceCoordinates (Vector3 world, GridPlane thePlane, HexCoordinateSystem coordianteSystem){
		if(coordianteSystem == HexCoordinateSystem.Herring){
			return GetFaceCoordinatesHerring(world, thePlane);
		} else{
			return Vector3.zero;
		}
	}
	protected Vector3 GetBoxCoordinates (Vector3 world, HexCoordinateSystem coordianteSystem){
		if(coordianteSystem == HexCoordinateSystem.Herring){
			return GetBoxCoordinatesHerring(world);
		} else{
			return Vector3.zero;
		}
	}
	#endregion
	
	//returns XYZ grid coordinates of a vertex close to a given point
	protected Vector3 GetVertexCoordinatesHerring(Vector3 world){
		Vector3 nearestVertex = FindNearestVertex(world);
		
		return WorldToGrid(nearestVertex);
	}
	
	//idea: cover the hex grid with a grid of rectangular tiles, find the position inside the tile and then
	// the hex that belongs to that point's tile
	protected Vector3 GetFaceCoordinatesHerring(Vector3 world, GridPlane thePlane){
		Vector3 tile = GetTileCoordinates(world, thePlane);
		Vector3 tilePoint = GetTilePointCoordinates(world, thePlane);
		Vector3 face = new Vector3();
		
		//there are three possible hexagons, find the one we need (left edge belongs to the hex, right one doesn't)
		face[idx[0]] = tilePoint[0] >= radius * Mathf.Abs(0.5f - tilePoint[1] / height) ? tile[idx[0]] : tile[idx[0]] - 1;
		int delta = tilePoint[1] > 0.5f * height ? 1 : 0;
		face[idx[1]] = tilePoint[0] >= radius * Mathf.Abs(0.5f - tilePoint[1] / height) ? tile[idx[1]] : tile[idx[1]] - Mathf.Abs(face[idx[0]] % 2) + delta;
		face[idx[2]] = tile[idx[2]];
		//Debug.Log(face);

		return face;
	}
	
	protected Vector3 GetBoxCoordinatesHerring(Vector3 world){
		Vector3 local = _transform.GFInverseTransformPointFixed(world);
		Vector3 box = GetFaceCoordinates(world, gridPlane);
		
		if(local[(int)gridPlane] >= 0){
			box[(int)gridPlane] = (local[(int)gridPlane] - local[(int)gridPlane] % depth) / depth;
		} else{
			box[(int)gridPlane] = (local[(int)gridPlane] - (depth + local[(int)gridPlane] % depth)) / depth;
		}
		
		//Debug.Log(box);
		return box;	
	}
	
	#region leftover
	// I think this returns the vertex in a vertex-based coordinate system (as opposed to the default face-based one)
	protected Vector3 GetVertexCoordinatesSomething(Vector3 world){
		Vector3 face = FindNearestFace(world, gridPlane); //find the nearest face in the world
		Vector3 localFace = GetFaceCoordinates(world, gridPlane); //the same face but with local coordinates
		// calculate the offset of the point from the centre of the hex (local 3D coordinates)
		Vector3 facePoint = _transform.GFInverseTransformPointFixed(world) - _transform.GFInverseTransformPointFixed(face); // Vector3 -> Vector2
		
		//inside the tile (hex) there are six possibilities, we need to find the right one
		HexVertex vert = GetHexVert(facePoint);
		
		Vector3 vertex = localFace;// use the centre of the hex as a starting point
//		Debug.Log("Face Point: " + facePoint + ", vertex: " + vert);
		
		vertex[idx[1]] = 2 * localFace[idx[1]];// the quasi-Y of a vertex is at least twice the quasi-Y of the face
		
		if(vert == HexVertex.W || vert == HexVertex.E){// move one step up
			vertex[idx[1]] ++;
		} else if(vert == HexVertex.NW || vert == HexVertex.NE){// move two steps up
			vertex[idx[1]] ++; vertex[idx[1]] ++;
		}
		
		if(vert == HexVertex.NE || vert == HexVertex.E || vert == HexVertex.SE)// move one step right
			vertex[idx[0]] ++;
		
		if(Mathf.Abs(localFace[idx[0]] % 2) > 0.1)// upwards shifted hexes add one step by default
			vertex[idx[1]] ++;
		//Debug.Log(vert);
		//Debug.Log(tile);
//		Debug.Log("vertex: " + vertex + ", face: " + localFace);
		
		return vertex;
	}
	#endregion
	
	#region helpers
	protected Vector3 GetTileCoordinates(Vector3 world, GridPlane thePlane){
		//get the world point into local space
		Vector3 local = _transform.GFInverseTransformPointFixed(world);
//		Debug.Log(local);
		Vector3 tile = new Vector3();
		
		//find the coordinates of the tile
		tile[idx[0]] = Mathf.Floor((local[idx[0]] + 1.0f * radius) / side);
		float shift = 1 - Mathf.RoundToInt(Mathf.Abs(tile[idx[0]]) % 2); // 1 for even, 0 for odd
		tile[idx[1]] = Mathf.Floor((local[idx[1]] + (shift * 0.5f * height)) / height);
		tile[idx[2]] = Mathf.Round(local[idx[2]] / depth); // maybe I stil need the above instead
		
//		Debug.Log(tile);
		return tile;
	}
	
	//the cordinates inside the tile
	protected Vector2 GetTilePointCoordinates(Vector3 world, GridPlane thePlane){
		Vector3 local = _transform.GFInverseTransformPointFixed(world);
		Vector3 tile = GetTileCoordinates(world, thePlane);
		Vector2 tilePoint = new Vector2();
		
		tilePoint[0] = (local[idx[0]] + 1.0f * radius) - tile[idx[0]] * side;
		float shift = 1 - Mathf.RoundToInt(Mathf.Abs(tile[idx[0]]) % 2); // 1 for even, 0 for odd
		tilePoint[1] = (local[idx[1]] + shift * 0.5f * height) - tile[idx[1]] * height;
//		Debug.Log(tilePoint);
		
		return tilePoint;
	}
	
	//pick the vertex using tilePoint information
	protected HexVertex GetHexVert(Vector3 facePoint){
		HexVertex vert;
		Vector2 angleVec = new Vector2();//this vector is a flat projections of the point onto the grid plane
		for(int i = 0; i < 2; i++){
			angleVec[i] = facePoint[idx[i]];
		}

		float angle = Vector2.Angle(angleVec, Vector2.right);
		
		if(angle < 30.0f){
			vert = HexVertex.E;
		} else if(90.0f > angle && angle >= 30.0f){
			vert = HexVertex.NE;
			if(angleVec[1] < 0)
				vert = HexVertex.SE;
		} else if(150.0f > angle && angle >= 90.0f){
			vert = HexVertex.NW;
			if(angleVec[1] < 0)
				vert = HexVertex.SW;
		} else{
			vert = HexVertex.W;
		}
//		Debug.Log(angleVec + ", " +angle);
		
		return vert;
	}
	#endregion
	#endregion
	
	
	#region VertexMatrixMethods

	public override Vector3[,,] BuildVertexMatrix(float height, float width, float depth){
		//prevent negative values
		width = Mathf.Abs(width);
		height = Mathf.Abs(height);
		depth = Mathf.Abs(depth);
		int[] matrixSize = new int[3]{Mathf.FloorToInt(width), Mathf.FloorToInt(height), Mathf.FloorToInt(depth)};
		
		Vector3 iterationVector = Vector3.zero;
		for(int i = 0; i <= 2; i++){
			iterationVector[i] = matrixSize[i];
		}
		
		Vector3[,,] vertexMatrix = new Vector3[2*matrixSize[0]+1, 4*matrixSize[1]+2, 2*matrixSize[2]+1];
		int yIterator = 0;
		
		for(int z = 0; z <= 2 * matrixSize[2]; z++){
			for(int x = 0; x <= 2 * matrixSize[0]; x++){
				for(int y = 0; y < 2 * matrixSize[1]; y++){
					bool shifted = matrixSize[0] % 2 == 1 ? x % 2 == 0 : x % 2 == 1;
					Vector3 hex = GridToWorld(new Vector3(-matrixSize[0], -matrixSize[1], -matrixSize[2]) + new Vector3(x, y, z));
				//	Debug.Log(hex);
				//	Gizmos.DrawSphere(hex, 0.4f);
					if(y == 0 && shifted){
						Vector3 leftHex = GridToWorld(new Vector3(-matrixSize[0], -matrixSize[1], -matrixSize[2]) + new Vector3(x-1, y, z));
						vertexMatrix[x, yIterator, z] = leftHex + VertexToDirection(HexVertex.SE);
						yIterator++;
					}
					vertexMatrix[x, yIterator, z] = hex + VertexToDirection(HexVertex.SW);
					yIterator++;
					vertexMatrix[x, yIterator, z] = hex + VertexToDirection(HexVertex.W);
					yIterator++;
					if(y == 2 * matrixSize[1] - 1){
						vertexMatrix[x, yIterator, z] = hex + VertexToDirection(HexVertex.NW);
						yIterator++;
					}
					if(y == 2 * matrixSize[1] - 1 && !shifted){
						Vector3 leftHex = GridToWorld(new Vector3(-matrixSize[0], -matrixSize[1], -matrixSize[2]) + new Vector3(x-1, y, z));
						vertexMatrix[x, yIterator, z] = leftHex + VertexToDirection(HexVertex.NE);
						yIterator++;
					}
				}
				yIterator = 0;
			}
		}
		
		return vertexMatrix;
	}
	
	public override Vector3 ReadVertexMatrix(int x, int y, int z, Vector3[,,] vertexMatrix, bool warning = false){
		if(Mathf.Abs(x)>vertexMatrix.GetUpperBound(0)/2 || Mathf.Abs(y) >vertexMatrix.GetUpperBound(1)/2 || Mathf.Abs(z) >vertexMatrix.GetUpperBound(2)/2){
			if(warning)
				Debug.LogWarning("coordinates too large for this matrix, will default to " + Vector3.zero);
			return vertexMatrix[(vertexMatrix.GetUpperBound(0)/2), (vertexMatrix.GetUpperBound(1)/2), (vertexMatrix.GetUpperBound(2)/2)];
		}
		return vertexMatrix[(vertexMatrix.GetUpperBound(0)/2) + x, (vertexMatrix.GetUpperBound(1)/2) + y, (vertexMatrix.GetUpperBound(2)/2) + z];
	}
		
	#endregion
	

	#region Align Scale Methods
	#region overloads
	public Vector3 AlignVector3(Vector3 pos, GFBoolVector3 lockAxis, bool beTolerant = true){
		return AlignVector3(pos, Vector3.one, lockAxis, beTolerant);
	}
	#endregion
	
	public override Vector3 AlignVector3(Vector3 pos, Vector3 scale, GFBoolVector3 lockAxis, bool beTolerant = true){
		Vector3 newPos = FindNearestFace(pos);
		for(int i = 0; i < 3; i++){
			if(lockAxis[i])
				newPos[i] = pos[i];
		}
		return newPos;
	}
	
	public override Vector3 ScaleVector3(Vector3 scl, GFBoolVector3 lockAxis){
		Vector3 spacing = new Vector3();
		for(int i = 0; i < 2; i++){
			spacing[idx[i]] = height;
		}
		spacing[idx[2]] = depth;
		Vector3 relScale = scl.GFModulo3(spacing);
		Vector3 newScale = new Vector3();
				
		for (int i = 0; i <= 2; i++){
			newScale[i] = scl[i];			
			if(relScale[i] >= 0.5f * spacing[i]){
//				Debug.Log ("Grow by " + (spacing.x - relScale.x));
				newScale[i] = newScale[i] - relScale[i] + spacing[i];
			} else{
//				Debug.Log ("Shrink by " + relativeScale.x);
				newScale[i] = newScale[i] - relScale[i];
				//if we went too far default to the spacing
				if(newScale[i] < spacing[i])
					newScale[i] = spacing[i];
			}
		}
		
		for(int i = 0; i < 3; i++){
			if(lockAxis[i])
				newScale[i] = scl[i];
		}
		
		return newScale;
	}
	
	#endregion
	

	#region Drawing Methods
		
	public override void DrawGrid(Vector3 from, Vector3 to){
		DrawGridRect(from, to);		
	}
	
	public void DrawGridRect(Vector3 from, Vector3 to){
		if(hideGrid)
			return;
		
		Vector3[][][] lines = CalculateDrawPointsRect(from, to, gridStyle == HexGridShape.CompactRectangle);
		
		//Swap the X and Y colours if the grid has flat sides (that's because I swapped quasi-X and quasi-Y when calculating the points)
		Swap<Color>(ref axisColors.x, ref axisColors.y, hexSideMode == HexOrientation.FlatSides);
		
		for(int i = 0; i < 3; i++){//looping through the two (three?) directions
			if(hideAxis[i])
				continue;
			Gizmos.color = axisColors[i];
			foreach(Vector3[] line in lines[i]){
			if(line != null)	Gizmos.DrawLine(line[0], line[1]);
			}
		}
		
		Swap<Color>(ref axisColors.x, ref axisColors.y, hexSideMode == HexOrientation.FlatSides); //swap the colours back
		
		//draw a sphere at the centre
		if(drawOrigin){
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(_transform.position, 0.3f);
		}
		
	}
		
	#endregion
	
	
	#region Render Methods
	#region overload
	public override void RenderGrid(Vector3 from, Vector3 to, GFColorVector3 colors, int width = 0, Camera cam = null, Transform camTransform = null){
		RenderGridRect(from, to, useSeparateRenderColor ? renderAxisColors : axisColors, width, cam, camTransform);
	}
	
	public void RenderGridRect(int width = 0, Camera cam = null, Transform camTransform = null){
		RenderGridRect(-size, size, useSeparateRenderColor ? renderAxisColors : axisColors, width, cam, camTransform);
	}
	#endregion
	
	public void RenderGridRect(Vector3 from, Vector3 to, GFColorVector3 colors, int width = 0, Camera cam = null, Transform camTransform = null){
		if(!renderGrid)
			return;
		
		if(!renderMaterial)
			renderMaterial = defaultRenderMaterial;
		
		Vector3[][][] lines = CalculateDrawPointsRect(from, to, gridStyle == HexGridShape.CompactRectangle);
		
		renderMaterial.SetPass(0);
		
		if(width <= 1 || !cam || !camTransform){// use simple lines for width 1 or if no camera was passed
			GL.Begin(GL.LINES);
			for(int i = 0; i < 3; i++){
				if(hideAxis[i])
					continue;
				GL.Color(colors[i]);
				foreach(Vector3[] line in lines[i]){
					if(line == null) continue;
					GL.Vertex(line[0]); GL.Vertex(line[1]);
				}
			}
			GL.End();
		} else{// quads for "lines" with width
			GL.Begin(GL.QUADS); // we will be working in Screen Space here
			float mult = Mathf.Max(0, 0.5f * width); //the multiplier, half the desired width
			
			for(int i = 0; i < 3; i++){
				GL.Color(colors[i]);
				if(hideAxis[i] || lines[i][0] == null)
					continue;
				
				//sample a direction vector, one per direction is enough (using the first line of each line set
				Vector3 dir = Vector3.Cross(lines[i][0][0] - lines[i][0][1], camTransform.forward).normalized;
				//multiply dir with the world length of one pixel in distance
				if(cam.orthographic){
					dir *= (cam.orthographicSize * 2) / cam.pixelHeight;
				} else{// (the 20 below is just there to smooth out rounding errors)
					dir *= (cam.ScreenToWorldPoint(20*new Vector3(0, 0, 1)) - cam.ScreenToWorldPoint(20*new Vector3(1, 0, 1))).magnitude/20;
				}
															
				foreach(Vector3[] line in lines[i]){
					if(line == null) continue;
					
					Vector3 vec1 = line[0];
					Vector3 vec2 = line[1];
					GL.Vertex(vec1-mult*dir); GL.Vertex(vec1+mult*dir); GL.Vertex(vec2+mult*dir); GL.Vertex(vec2-mult*dir);
				}
			}
			GL.End();
		}
	}
	
	#endregion
	
	
	#region Draw Gizoms
	
	public void OnDrawGizmos(){
		if(useCustomRenderRange){
			DrawGrid(renderFrom, renderTo);
		} else{
			DrawGrid();
		}
		//Gizmos.DrawSphere(GridToWorld(Vector3.zero), 0.3f);
		//DrawHerring();
	}
	
	protected void DrawHerring(){
		DrawHerring(-size, size);
	}
	
	protected void DrawHerring(Vector3 from, Vector3 to){
		for(int i = Mathf.FloorToInt(from[idx[0]] / (1.5f * radius)) + 1; i < Mathf.FloorToInt(to[idx[0]] / (1.5f * radius)); i++){
			for(int j = Mathf.FloorToInt(from[idx[1]] / height) + 1; j < Mathf.FloorToInt(to[idx[1]] / height); j++){
				for(int k = Mathf.FloorToInt(from[idx[2]] / depth); k < Mathf.FloorToInt(to[idx[2]] / depth) + 1; k++){
					Gizmos.color = Color.yellow;
					Gizmos.DrawLine(GridToWorld(i * units[idx[0]] + j * units[idx[1]] + k * units[idx[2]]),
						GridToWorld((i+1) * units[idx[0]] + j * units[idx[1]] + k * units[idx[2]]));
					Gizmos.color = Color.white;
					Gizmos.DrawLine(GridToWorld(i * units[idx[0]] + j * units[idx[1]] + k * units[idx[2]]),
						GridToWorld(i * units[idx[0]] + (j+1) * units[idx[1]] + k * units[idx[2]]));
				}
			}
		}
	}
	
	#endregion
	
	
	//calculates the points to be used for drawing and rendering (the result is of type Vector3[][][] where the most inner array is a pair of two points,
	//the middle array is the set of all points of the same axis and the outer array is the set of those three sets
	
	#region Calculate draw points
	
	#region overload
	protected override Vector3[][][] CalculateDrawPoints(Vector3 from, Vector3 to){
		return CalculateDrawPointsRect(from, to, gridStyle == HexGridShape.CompactRectangle);
	}
	
	protected Vector3[][][] CalculateDrawPointsRect(bool isCompact = false){
		return CalculateDrawPointsRect(-size, size, gridStyle == HexGridShape.CompactRectangle);
	}
	
	protected void CalculateDrawPointsRhomb(){
		CalculateDrawPointsRhomb(-size, size);
	}
	
	protected void CalculateDrawPointsTriangle(){
		CalculateDrawPointsTriangle(-size, size);
	}
	
	protected void CalculateDrawPointsBigHex(){
		CalculateDrawPointsBigHex(-size, size);
	}
	#endregion
	
	protected Vector3[][][] CalculateDrawPointsRect(Vector3 from, Vector3 to, bool isCompact = false){
		// reuse the points if the grid hasn't changed, we already have some points and we use the same range
		if(!hasChanged && _drawPoints != null && from == renderFrom && to == renderTo && isCompact == (gridStyle == HexGridShape.CompactRectangle)){
			return _drawPoints;
		}
		
		_drawPoints = new Vector3[3][][];
		Vector3 spacing = CombineRadiusDepth();
		Vector3 relFrom = relativeSize ? Vector3.Scale(from, spacing) : from;
		Vector3 relTo = relativeSize ? Vector3.Scale(to, spacing) : to;
		
		float[] length = new float[3];
		for(int i = 0; i < 3; i++){length[i] = relTo[i] - relFrom[i];}
		
		//calculate the amount of steps from the centre for the first hex (I will need these values later)
		int startX = Mathf.FloorToInt(relTo[idx[0]] / (1.5f * radius));
		int startY = Mathf.FloorToInt(relTo[idx[1]] / height);
		
		int endX = Mathf.CeilToInt(relFrom[idx[0]] / (1.5f * radius));
		int endY = Mathf.CeilToInt(relFrom[idx[1]] / height);
						
		//the starting point of the first pair (an iteration vector will be added to this)
		Vector3[] startPoint = new Vector3[1]{ // can this be expanded to use 3 points and draw incomplete lines like in RectGrid?
			//everything in the right top front
			_transform.position + unitVectors[idx[0]] * (1.5f * startX) * radius
				+ unitVectors[idx[1]] * ((startY + (Mathf.Abs(startX % 2)) * 0.5f) * height)
				+ unitVectors[idx[2]] * depth * Mathf.Floor(relTo[idx[2]] / depth)
		};
		//Gizmos.DrawSphere(startPoint[0], 0.3f);
				
		int[] amount = new int[3]{
			startX - endX + 1,
			startY - endY + 1,
			Mathf.FloorToInt(relTo[idx[2]] / depth) - Mathf.CeilToInt(relFrom[idx[2]] / depth) + 1
		};
				
		//a multiple of this will be added to the starting point for iteration
		Vector3[] iterationVector = new Vector3[3]{
			unitVectors[idx[0]] * -side, unitVectors[idx[1]] * -height,	unitVectors[idx[2]] * -depth
		};
		
		Vector3[][] lineSetX = new Vector3[(amount[0] * amount[1] + amount[0])  * amount[2]][];
		Vector3[][] lineSetY = new Vector3[(amount[0] * 2 * amount[1] + 2 * amount[1] + amount[0] - 1) * amount[2]][];
		
		int[] iterator = new int[3]{0, 0, 0};
		
		for(int i = 0; i < amount[2]; i++){ //loop through the quasi-Z axis
			for(int j = 0; j < amount[0]; j++){ // loop through the quasi-X axis
				bool isShiftedUp = (startX % 2 == 0 && j % 2 == 1) || (Mathf.Abs(startX % 2) == 1 && j % 2 == 0); //is the current hex shifted upwards?
				for(int k = 0; k < amount[1]; k++){  // loop through the quasi-Y axis
					Vector3 hexCentre = startPoint[0] + j * iterationVector[0] + k * iterationVector[1] + i * iterationVector[2];
					//quasi-Y offset adjusting (can this be made into its own variable?)
					if(startX % 2 == 0 && j % 2 == 1){
						hexCentre += 0.5f * height * unitVectors[idx[1]];
					} else if(Mathf.Abs(startX % 2) == 1 && j % 2 == 1){
						hexCentre -= 0.5f * height * unitVectors[idx[1]];
					}
					
					Vector3[] lineSE = new Vector3[2]{hexCentre + unitVectors[idx[0]] * radius,
						hexCentre - unitVectors[idx[1]] * 0.5f * height + unitVectors[idx[0]] * 0.5f * radius};
					if(!(isShiftedUp && k == 0 && j == 0 && isCompact)){
						lineSetY[iterator[1]] = lineSE; iterator[1]++; //make an exception in one case
					}
					
					Vector3[] lineS = new Vector3[2]{lineSE[1], lineSE[1] - radius * unitVectors[idx[0]]};
					lineSetX[iterator[0]] = lineS; iterator[0]++;
					
					//if the grid is compact we don't need the rest from the up shifted upper lines for the first run of the inner loop
					if(isCompact && k == 0 && isShiftedUp)
						continue;
					
					Vector3[] lineNE = new Vector3[2]{lineSE[0] + 0.5f * height * unitVectors[idx[1]] - 0.5f * radius * unitVectors[idx[0]], lineSE[0]};
					lineSetY[iterator[1]] = lineNE; iterator[1]++;
					
					
					if(k == 0){ // all upper hexes get one northern line (this gives us all missing quasi-X lines)
						Vector3[] lineN = new Vector3[2]{lineS[1] + height * unitVectors[idx[1]],lineS[0] + height * unitVectors[idx[1]]};
						lineSetX[iterator[0]] = lineN; iterator[0]++;
					}
					
					if(j == amount[0] - 1){ // all the left hexes get a north-western line
						Vector3[] lineNW = new Vector3[2]{lineSE[1] - side * unitVectors[idx[0]] + 0.5f * height * unitVectors[idx[1]],
							lineSE[0] - side * unitVectors[idx[0]] + 0.5f * height * unitVectors[idx[1]]};
						lineSetY[iterator[1]] = lineNW; iterator[1]++;
					}
					
					// north-western lines for upper shifted hexes (except the most left one, that has been dealt with above)
					if(isShiftedUp && k == 0 && j != amount[0] - 1){
						Vector3[] lineNW = new Vector3[2]{lineSE[1] - side * unitVectors[idx[0]] + 0.5f * height * unitVectors[idx[1]],
							lineSE[0] - side * unitVectors[idx[0]] + 0.5f * height * unitVectors[idx[1]]};
						lineSetY[iterator[1]] = lineNW; iterator[1]++;
					}
					
					if(j == amount[0] - 1){ // all the left hexes get a south-western line
						Vector3[] lineSW = new Vector3[2]{lineSE[1] - radius * unitVectors[idx[0]], lineSE[0] - 2.0f * radius * unitVectors[idx[0]]};
						lineSetY[iterator[1]] = lineSW; iterator[1]++;
					}
					
					// south-western lines for lower unshifted hexes (except the most left one, that has been dealt with above)
					if(!isShiftedUp && k == amount[1] - 1 && j != amount[0] - 1){
						Vector3[] lineSW = new Vector3[2]{lineSE[1] - radius * unitVectors[idx[0]], lineSE[0] - 2.0f * radius * unitVectors[idx[0]]};
						lineSetY[iterator[1]] = lineSW; iterator[1]++;
					}
					
					//Gizmos.DrawSphere(hexCentre, 0.3f);
				}
			}
		}
		_drawPoints[0] = lineSetX;
		_drawPoints[1] = lineSetY;
		
		Vector3[][] lineSetZ = new Vector3[2 * amount[0] * amount[1] + 3 * amount[0] + 2 * amount[1] - 1][];
		//similar to above loop though all the hexes and add each vertex once
		for(int j = 0; j < amount[0]; j++){
			bool isShiftedUp = (startX % 2 == 0 && j % 2 == 1) || (Mathf.Abs(startX % 2) == 1 && j % 2 == 0);
			Vector3 depthShift = - unitVectors[idx[2]] * length[idx[2]];
			Vector3 hexStart = startPoint[0] + unitVectors[idx[2]] * (relTo[idx[2]] - depth * Mathf.Floor(relTo[idx[2]] / depth)); //adjust quasi-Z coordinate
			for(int k = 0; k < amount[1]; k++){
				Vector3 hexCentre = hexStart + j * iterationVector[0] + k * iterationVector[1];
				
				//quasi-Y offset adjusting (can this be made into its own variable?)
				if(startX % 2 == 0 && j % 2 == 1){
					hexCentre += 0.5f * height * unitVectors[idx[1]];
				} else if(Mathf.Abs(startX % 2) == 1 && j % 2 == 1){
					hexCentre -= 0.5f * height * unitVectors[idx[1]];
				}
				
				Vector3 pointE = hexCentre + unitVectors[idx[0]] * radius;
				if(!(isShiftedUp && k == 0 && j == 0 && isCompact)){//make an exception in one case
					Vector3[] pairE = new Vector3[2]{pointE, pointE + depthShift};
					lineSetZ[iterator[2]] = pairE; iterator[2]++;
					//Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f); Gizmos.DrawSphere(pointE, 0.3f);
				}
				Vector3 pointSE = hexCentre - unitVectors[idx[1]] * 0.5f * height + unitVectors[idx[0]] * 0.5f * radius;
				Vector3[] pairSE = new Vector3[2]{pointSE, pointSE + depthShift};
				lineSetZ[iterator[2]] = pairSE; iterator[2]++;
				//Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.3f); Gizmos.DrawSphere(pointSE, 0.3f);
				
				//if the grid is compact we don't need the rest from the up shifted upper lines
				if(isCompact && k == 0 && isShiftedUp)
					continue;
				
				if(k == 0){ // all upper hexes get one north-eastern point
					Vector3 pointNE = pointSE + height * unitVectors[idx[1]];
					Vector3[] pairNE = new Vector3[2]{pointNE, pointNE + depthShift};
					lineSetZ[iterator[2]] = pairNE; iterator[2]++;
					//Gizmos.color = new Color(1, 1, 0, 0.3f); Gizmos.DrawSphere(pointNE, 0.3f);
				}
				
				if(j == amount[0] - 1){ // all the left hexes get a north-western point
					Vector3 pointNW = pointE - side * unitVectors[idx[0]] + 0.5f * height * unitVectors[idx[1]];
					Vector3[] pairNW = new Vector3[2]{pointNW, pointNW + depthShift};
					lineSetZ[iterator[2]] = pairNW; iterator[2]++;
					//Gizmos.color = new Color(0, 1, 1, 0.3f); Gizmos.DrawSphere(pointNW, 0.3f);
				}
				
				// north-western points for upper shifted hexes (except the most left one, that has been dealt with above)
				if(isShiftedUp && k == 0 && j != amount[0] - 1){
					Vector3 pointNW = pointE - side * unitVectors[idx[0]] + 0.5f * height * unitVectors[idx[1]];
					Vector3[] pairNW = new Vector3[2]{pointNW, pointNW + depthShift};
					lineSetZ[iterator[2]] = pairNW; iterator[2]++;
					//Gizmos.DrawSphere(pointNW, 0.3f); Gizmos.color = new Color(1, 0, 1, 0.3f); //Magenta
				}
				
				if(j == amount[0] - 1){ // all the left hexes get a western point (and a south-western for the lowest one)
					Vector3 pointW = pointE - 2.0f * radius * unitVectors[idx[0]];
					Vector3[] pairW = new Vector3[2]{pointW, pointW + depthShift};
					lineSetZ[iterator[2]] = pairW; iterator[2]++;
					//Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 0.3f);Gizmos.DrawSphere(pointW, 0.3f);
					if(k == amount[1] - 1){ // one more for the lower left hex
						Vector3 pointSW = pointSE - radius * unitVectors[idx[0]];
						Vector3[] pairSW = new Vector3[2]{pointSW, pointSW + depthShift};
						lineSetZ[iterator[2]] = pairSW; iterator[2]++;
						//Gizmos.color = new Color(0, 0, 0, 0.3f); Gizmos.DrawSphere(pointSW, 0.3f);
					}
				}
				
				// south-western points for lower unshifted hexes (except the most left one, that has been dealt with above)
				if(!isShiftedUp && k == amount[1] - 1 && j != amount[0] - 1){
					Vector3 pointSW = pointSE - radius * unitVectors[idx[0]];
					Vector3[] pairSW = new Vector3[2]{pointSW, pointSW + depthShift};
					lineSetZ[iterator[2]] = pairSW; iterator[2]++;
					//Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); Gizmos.DrawSphere(pointSW, 0.3f);
				}
			}
		}
		//Debug.Log((2 * amount[0] * amount[1] + 3 * amount[0] + 2 * amount[1] - 1 - iterator[3]) + "= " + (2 * amount[0] * amount[1] + 3 * amount[0] + 2 * amount[1] - 1) + " - " + iterator[3]);
		_drawPoints[2] = lineSetZ;		
		
		return _drawPoints;
	}
	
	protected void CalculateDrawPointsRhomb(Vector3 from, Vector3 to){
		
	}
	
	protected void CalculateDrawPointsTriangle(Vector3 from, Vector3 to){
		
	}
	
	protected void CalculateDrawPointsBigHex(Vector3 from, Vector3 to){
		
	}
	
	#endregion

//_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
	
	#region helper functions
	
	//transforms from quasi axis to real axis. Quasi axis is the relative X, Y and Z n the current grid plane,
	// all calculations are done in quasi space, so there is only one calculation, and then transformed into real space
	protected int[] TransformIndices(GridPlane plane){
		int[] indices = new int[3];
		if(plane == GridPlane.YZ){
			indices[0] = 2; indices[1] = 1; indices[2] = (int)gridPlane;
		} else if(plane == GridPlane.XZ){
			indices[0] = 0; indices[1] = 2; indices[2] = (int)gridPlane;
		} else{
			indices[0] = 0; indices[1] = 1; indices[2] = (int)gridPlane;
		}
		Swap<int>(ref indices[0], ref indices[1], hexSideMode == HexOrientation.FlatSides);
		return indices;
	}
	
	// returns the direction from a face to the specified vertex (world space only!)
	protected Vector3 VertexToDirection(HexVertex vert, bool worldSpace = true){
		Vector3 dir = new Vector3();
				
		if(vert == HexVertex.E || vert == HexVertex.W){
			dir = unitVectors[idx[0]] * radius;
			if(vert == HexVertex.W)
				dir = -dir;
		} else if(vert == HexVertex.N || vert == HexVertex.S){
			dir = unitVectors[idx[1]] * 0.5f * height;
			if(vert == HexVertex.S)
				dir = -dir;
		} else if(vert == HexVertex.NE || vert == HexVertex.SW){
			dir = 0.5f * radius * unitVectors[idx[0]] + 0.5f * height * unitVectors[idx[1]];
			if(vert == HexVertex.SW)
				dir = -dir;
		}  else if(vert == HexVertex.NW || vert == HexVertex.SE){
			dir = -0.5f * radius * unitVectors[idx[0]] + 0.5f * height * unitVectors[idx[1]];
			if(vert == HexVertex.SE)
				dir = -dir;
		}
		
		return dir;	
	}
	
	// combines radius and depth into one vector that works like spacing for rectangular grids
	protected Vector3 CombineRadiusDepth(){
		Vector3 spacing = new Vector3();
		spacing[idx[0]] = 1.5f * radius;
		spacing[idx[1]] = Mathf.Sqrt(3) * radius;
		spacing[idx[2]] = depth;
		return spacing;
	}
	
	//swaps two variables, useful for swapping quasi-X and quasi-Y to keep the same formula for pointy sides and flat sides
	void Swap<T>(ref T a, ref T b, bool condition = true){
		if(condition){
			T temp = b; b = a; a = temp;
		}
	}
	
	#endregion
}
