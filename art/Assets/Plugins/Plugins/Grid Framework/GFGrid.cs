using UnityEngine;
using System.Collections;
using System.Linq;

[System.Serializable]
public abstract class GFGrid : MonoBehaviour {
	
	# region size and range
	public bool relativeSize = false;
	
	[SerializeField]
	private Vector3 _size = new Vector3(5.0f, 5.0f, 5.0f);
	public Vector3 size{
		get{return _size;}
		set{if(value == _size)// needed because the editor fires the setter even if this wasn't changed
				return;
			_gridChanged = true;
			_size = Vector3.Max(value, Vector3.zero);}
	}
	
	[SerializeField]
	private Vector3 _renderFrom = Vector3.zero; //first one from, second one to 
	public Vector3 renderFrom{
		get{return _renderFrom;}
		set{if(value == _renderFrom)// needed because the editor fires the setter even if this wasn't changed
				return;
			_gridChanged = true;
			_renderFrom = Vector3.Min(value, renderTo);}
	}
	[SerializeField]
	private Vector3 _renderTo = 3*Vector3.one; //first one from, second one to 
	public Vector3 renderTo{
		get{return _renderTo;}
		set{if(value == _renderTo)// needed because the editor fires the setter even if this wasn't changed
				return;
			_gridChanged = true;
			_renderTo = Vector3.Max(value, _renderFrom);}
	}
	#endregion
	
	//the index refers to the missing axis (X=0, Y=1, Z=2)
	public enum GridPlane {YZ, XZ, XY};
	
	private enum AlignReference {Center, RightUpBack, RightUpFront, RightDownBack, RightDownFront, LeftUpBack, LeftUpFront, LeftDownBack, LeftDownFront};
		
	//each grid gets its own vertex matrix
	public Vector3[,,] ownVertexMatrix;
	
	protected Vector3[][][] _drawPoints;

	public GFColorVector3 axisColors = new GFColorVector3();
	public Color vertexColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
	public bool useSeparateRenderColor = false;
	public GFColorVector3 renderAxisColors = new GFColorVector3(Color.gray);

	public bool hideGrid = false;
	public bool hideOnPlay = false;
	public GFBoolVector3 hideAxis = new GFBoolVector3();
	public bool drawOrigin = false;
	
	public bool renderGrid = true;
	[SerializeField]
	protected bool _useCustomRenderRange = false;
	public bool useCustomRenderRange{get{return _useCustomRenderRange;}set{if(value == _useCustomRenderRange){return;} _useCustomRenderRange = value; _gridChanged = true;}}
	[SerializeField]
	protected int _renderLineWidth = 1;
	public int renderLineWidth{
		get{return _renderLineWidth;}
		set{_renderLineWidth = Mathf.Max(value, 1);}
	}
	public Material renderMaterial = null;
	
	protected Material defaultRenderMaterial{get{return new Material( "Shader \"Lines/Colored Blended\" {" +
		"SubShader { Pass { " +
		"	Blend SrcAlpha OneMinusSrcAlpha " +
		"	ZWrite Off Cull Off Fog { Mode Off } " +
		"	BindChannels {" +
		"	Bind \"vertex\", vertex Bind \"color\", color }" +
		"} } }" );}}
	
	#region caching and optimization
	// caching the transform for performance
	protected Transform cachedTransform; //this is the real cache
	// this is used for access, if there is nothing cached it performs the cache first, then return the component
	protected Transform _transform{get{if(!cachedTransform){cachedTransform = transform;}return cachedTransform;}}
	
	// this will be set to true whenever one of the accessors calls its set
	protected bool _gridChanged = false;
	protected bool hasChanged{
		get{
			if(_gridChanged || transformCache.needRecalculation){
				_gridChanged = false;
				transformCache.Recache();
				return true;
			} else{
				return _gridChanged;
			}
		}
	}
	
	protected class TransformCache{
		protected Transform _gridTransform;
		// these parts can be shortened using Transform.hasChanged, but that is Unity 4+ only
		public Vector3 oldPosition;
		public Quaternion oldRotation;
		
		// true if values have changed, false if they re the same
		public bool needRecalculation{get{return !(oldPosition == _gridTransform.position && oldRotation == _gridTransform.rotation);}}
		
		public void Recache(){
			oldPosition = _gridTransform.position;
			oldRotation = _gridTransform.rotation;
		}
		
		public TransformCache(GFGrid grid){
			_gridTransform = grid._transform;
			Recache();
		}
	}
	
	private TransformCache _transformCache;
	protected TransformCache transformCache{get{if(_transformCache == null){_transformCache = new TransformCache(this);}return _transformCache;}}
	#endregion
	
	
	#region Grid <-> World coordinate transformation
	public abstract Vector3 WorldToGrid(Vector3 gridPoint);
	public abstract Vector3 GridToWorld(Vector3 gridPoint);
	#endregion
	
	#region FindNearest
	public abstract Vector3 FindNearestVertex(Vector3 fromPoint, bool doDebug = false);
	public abstract Vector3 FindNearestFace(Vector3 fromPoint, GridPlane thePlane, bool doDebug = false);
	public abstract Vector3 FindNearestBox(Vector3 fromPoint, bool doDebug = false);
	#endregion
	
	#region GetCoordinates
	public abstract Vector3 GetVertexCoordinates(Vector3 world);
	public abstract Vector3 GetFaceCoordinates(Vector3 world, GridPlane thePlane);
	public abstract Vector3 GetBoxCoordinates(Vector3 world);
	#endregion
	
	#region vertex matrix
	public Vector3[,,] BuildVertexMatrix(Vector3 matrixSize){
		return BuildVertexMatrix(matrixSize.x, matrixSize.y, matrixSize.z);
	}
	public abstract Vector3[,,] BuildVertexMatrix(float height, float width, float depth);
	public abstract Vector3 ReadVertexMatrix(int i, int j, int k, Vector3[,,] vertexMatrix, bool warning = false);
	#endregion
	
	#region Align Methods
	#region overload
	public void AlignTransform(Transform theTransform){
		AlignTransform(theTransform, true, new GFBoolVector3(false), true);
	}
	
	public void AlignTransform(Transform theTransform, GFBoolVector3 lockAxis){
		AlignTransform(theTransform, true, lockAxis, true);
	}
	
	public void AlignTransform(Transform theTransform, bool rotate){
		AlignTransform(theTransform, rotate, new GFBoolVector3(false), true);
	}
	
	public void AlignTransform(Transform theTransform, bool rotate, GFBoolVector3 lockAxis){
		AlignTransform(theTransform, rotate, lockAxis, true);
	}
	
	public void AlignTransform(Transform theTransform, bool rotate, bool beTolerant){
		AlignTransform(theTransform, rotate, new GFBoolVector3(false), true);
	}
	
	public Vector3 AlignVector3(Vector3 pos){
		return AlignVector3(pos, Vector3.one, new GFBoolVector3(false), true);
	}
	
	public Vector3 AlignVector3(Vector3 pos, GFBoolVector3 lockAxis){
		return AlignVector3(pos, Vector3.one, lockAxis, true);
	}
	
	public Vector3 AlignVector3(Vector3 pos, Vector3 scale){
		return AlignVector3(pos, scale, new GFBoolVector3(false), true);
	}
	
	public Vector3 AlignVector3(Vector3 pos, Vector3 scale, bool beTolerant){
		return AlignVector3(pos, scale, new GFBoolVector3(false), beTolerant);
	}
		
	public Vector3 AlignVector3(Vector3 pos, Vector3 scale, GFBoolVector3 lockAxis){
		return AlignVector3(pos, scale, lockAxis, true);
	}
	#endregion
	public void AlignTransform(Transform theTransform, bool rotate, GFBoolVector3 lockAxis, bool beTolerant){
		Quaternion oldRotation = theTransform.rotation;
		theTransform.rotation = transform.rotation;

		theTransform.position = AlignVector3(theTransform.position, theTransform.lossyScale, lockAxis, beTolerant);
		if(!rotate)
			theTransform.rotation = oldRotation;
	}
	public abstract Vector3 AlignVector3(Vector3 pos, Vector3 scale, GFBoolVector3 lockAxis, bool beTolerant);
	#endregion
	
	#region Scale Methods
	#region overload
	public void ScaleTransform(Transform theTransform){
		ScaleTransform(theTransform, new GFBoolVector3(false));
	}
	public Vector3 ScaleVector3(Vector3 scl){
		return ScaleVector3(scl, new GFBoolVector3(false));
	}
	#endregion
	public void ScaleTransform(Transform theTransform, GFBoolVector3 lockAxis){
		theTransform.localScale = ScaleVector3(theTransform.localScale, lockAxis);
	}
	public abstract Vector3 ScaleVector3(Vector3 scl, GFBoolVector3 lockAxis);
	#endregion
	
	#region Render Methods
	#region overload
	public void RenderGrid(int width = 0, Camera cam = null, Transform camTransform = null){
		RenderGrid(-size, size, useSeparateRenderColor ? renderAxisColors : axisColors, width, cam, camTransform);
	}
	public void RenderGrid(Vector3 from, Vector3 to, int width = 0, Camera cam = null, Transform camTransform = null){
		RenderGrid(from, to, useSeparateRenderColor ? renderAxisColors : axisColors, width, cam, camTransform);
	}
	#endregion
	public abstract void RenderGrid(Vector3 from, Vector3 to, GFColorVector3 colors, int width = 0, Camera cam = null, Transform camTransform = null);
	#endregion
	
	#region Draw Methods
	public void DrawGrid(){
		DrawGrid(-size, size);
	}
	public abstract void DrawGrid(Vector3 from, Vector3 to);
	
	public void DrawVertices(Vector3[,,] vertexMatrix, bool drawOnPlay = false){
		//do not draw vertices when playing, this is a performance hog
		if(Application.isPlaying && !drawOnPlay)
		return;
		
		Gizmos.color = vertexColor;
		
		for(int i=0;  i<= vertexMatrix.GetUpperBound(0); i++){
			for(int j=0;  j<= vertexMatrix.GetUpperBound(1); j++){
				for(int k=0;  k<= vertexMatrix.GetUpperBound(2); k++){
					Gizmos.DrawSphere(vertexMatrix[i,j,k], 0.3f);
				}
			}
		}
	}
	#endregion
	
	#region Calculate Draw Points
	#region overload
	protected Vector3[][][] CalculateDrawPoints(){
		return CalculateDrawPoints(-size, size);
	}
	#endregion
	protected abstract Vector3[][][] CalculateDrawPoints(Vector3 from, Vector3 to);
	#endregion
	
	#region Vectrosity Methods
	#region overload
	public Vector3[] GetVectrosityPoints(){
		return GetVectrosityPoints(-size, size);
	}
	
	public Vector3[][] GetVectrosityPointsSeparate(){
		return GetVectrosityPointsSeparate(-size, size);
	}
	#endregion
	public Vector3[] GetVectrosityPoints(Vector3 from, Vector3 to){
		Vector3[][] seperatePoints = GetVectrosityPointsSeparate(from, to); 
		Vector3[] returnedPoints = new Vector3[seperatePoints[0].Length + seperatePoints[1].Length + seperatePoints[2].Length];
		seperatePoints[0].CopyTo(returnedPoints, 0);
		seperatePoints[1].CopyTo(returnedPoints, seperatePoints[0].Length);
		seperatePoints[2].CopyTo(returnedPoints, seperatePoints[0].Length + seperatePoints[1].Length);
		return returnedPoints;
	}
	
	public Vector3[][] GetVectrosityPointsSeparate(Vector3 from, Vector3 to){
		CalculateDrawPoints(from, to);
		int[] lengths = new int[3];
		for(int i = 0; i < 3; i++){
			lengths[i] = _drawPoints[i].Count(line => line != null)*2; // <- ! watch out, using LINQ !
		}
		
		Vector3[][] returnedPoints = new Vector3[3][];
		for(int i = 0; i < 3; i++){
			returnedPoints[i] = new Vector3[lengths[i]];
//			Debug.Log(lengths[i]);
		}
		int iterator = 0;
		for(int i = 0; i < 3; i++){
			iterator = 0;
			foreach(Vector3[] line in _drawPoints[i]){
				if(line == null)
					continue;
				returnedPoints[i][iterator] = line[0];
				iterator++;
				returnedPoints[i][iterator] = line[1];
				iterator++;
			}
		}
		
		return returnedPoints;
	}
	#endregion
	
	void Awake(){
		if(hideOnPlay)
			hideGrid = true;
		
		if(renderMaterial == null)
			renderMaterial = defaultRenderMaterial;
		
		GFGridRenderManager.AddGrid(this);
	}
	
	void OnDestroy(){
	    GFGridRenderManager.RemoveGrid(GetComponent<GFGrid>());
	}
	
	void Update(){
		
//		Debug.Log(_gridChanged);
	}
}

#region ColorVector3 class
[System.Serializable]
public class GFColorVector3{
	public Color x;
	public Color y;
	public Color z;
	
	// use an indexer to access the values (allows looping though them)
	public Color this[int i]{
		get{if(i == 0){
				return x;
			} else if(i == 1){
				return y;
			} else if(i == 2){
				return z;
			} else{
				return Color.white;
			}
		}
		set{switch(i){
				case 0: x = value;break;
				case 1: y = value;break;
				case 2: z = value;break;
			}}
	}
		
	//constructors
	public GFColorVector3(){ //default
		x = new Color(1.0f, 0.0f, 0.0f, 0.5f);
		y = new Color(0.0f, 1.0f, 0.0f, 0.5f);
		z = new Color(0.0f, 0.0f, 1.0f, 0.5f);
	}
	
	public GFColorVector3(Color color){ //one colour for everything
		x = color; y = color; z = color;
	}
	
	public GFColorVector3(Color xColor, Color yColor, Color zColor){ //taking individual colours
		x = xColor; y = yColor;	 z = zColor;
	}
}
#endregion

#region BoolVector3 class
[System.Serializable]
public class GFBoolVector3 {
	public bool x;
	public bool y;
	public bool z;
	
	// use an indexer to access the values (allows looping though them)
	public bool this[int i]{
		get{if(i == 0){
				return x;
			} else if(i == 1){
				return y;
			} else if(i == 2){
				return z;
			} else{
				return false;
			}
		}
		set{switch(i){
				case 0: x = value;break;
				case 1: y = value;break;
				case 2: z = value;break;
			}}
	}
	
	//constructors
	public GFBoolVector3(){
		x = false; y = false; z = false;
	}
	public GFBoolVector3(bool condition){// one state for everything
		x = condition; y = condition; z = condition;
	}	
	public GFBoolVector3(bool xBool, bool yBool, bool zBool){
		x = xBool; y = yBool; z = zBool;
	}
}
#endregion