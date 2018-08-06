using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;
public class SlidingPuzzleGrid : MonoBehaviour {
   
    // Awake is being called before Start; this makes sure we have a matrix to begin with when we add the blocks
    void Awake() {
		// because of how we wrote the accessor this will also immediately build the matrix of our level

        GameObject.Find("Obstacle").SetActive(true);
        GameObject.Find("SceneRoot").SetActive(false);

		SlidingPuzzleExample.mainGrid = gameObject.GetComponent<GFRectGrid>();

	}
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) {
            DirectoryInfo info = new DirectoryInfo(System.Environment.CurrentDirectory);
            FileStream nFile = new FileStream(info.Parent.Parent.FullName + @"\word\config\excelfile\map" + "/" +"data_map_"+ SceneManager.GetActiveScene().name + ".erl", FileMode.Create);
            Encoding encoder = Encoding.UTF8;
            byte[] bytes = encoder.GetBytes(SlidingPuzzleExample.MatrixToString());
            nFile.Write(bytes, 0, bytes.Length);
            nFile.Close();
            Debug.Log(info.Parent.Parent.FullName + @"\word\config\excelfile\server" + "/" + SceneManager.GetActiveScene().name + ".erl");

            DirectoryInfo luainfo = new DirectoryInfo(System.Environment.CurrentDirectory);
            FileStream luaFile = new FileStream(info.Parent.Parent.FullName + @"\client\art\Assets\MapConfigFile" + "/" + "Map" + SceneManager.GetActiveScene().name + ".lua", FileMode.Create);
            byte[] luabytes = encoder.GetBytes(SlidingPuzzleExample.BuildLua());
            luaFile.Write(luabytes, 0, luabytes.Length);
            luaFile.Close();
        }
    }
    // visualizes the matrix in text form to let you see what's going on
    void OnGUI(){
		//GUI.TextArea (new Rect (50, 50, 500, 500), SlidingPuzzleExample.ShowMatrixToString());
	}
}