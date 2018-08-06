// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// C # manual conversion work by Yun Kyu Choi

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

enum SaveFormat { Triangles, Quads }
enum SaveResolution { Full = 0, Half, Quarter, Eighth, Sixteenth }

struct ObjMaterial
{
    public string name;
    public string textureName;
}

class ExportTerrain : EditorWindow
{
    private static int vertexOffset = 0;
    private static int normalOffset = 0;
    private static int uvOffset = 0;
    private static string targetFolder = "ExportedObj";

    SaveFormat saveFormat = SaveFormat.Triangles;
    SaveResolution saveResolution = SaveResolution.Half;

    static ExportTerrain instance;
    static TerrainData terrain;
    static Vector3 terrainPos;

    int tCount;
    int counter;
    int totalCount;
    int progressUpdateInterval = 10000;

    GameObject[] passableObjs = null;
    [MenuItem("Terrain/Export To Obj...")]
    static void Init()
    {
        terrain = null;
        Terrain terrainObject = GameObject.FindGameObjectWithTag("Terrain").GetComponent<Terrain>() as Terrain;
        if (!terrainObject)
        {
            terrainObject = Terrain.activeTerrain;
        }
        if (terrainObject)
        {
            terrain = terrainObject.terrainData;
            terrainPos = terrainObject.transform.position;
        }
        
        instance = EditorWindow.GetWindow<ExportTerrain>();
        instance.Show();
        instance.passableObjs = GameObject.FindGameObjectsWithTag("Passable");

    }

    void OnGUI()
    {
        if (!terrain)
        {
            GUILayout.Label("No terrain found");
            if (GUILayout.Button("Cancel"))
            {
                EditorWindow.GetWindow<ExportTerrain>().Close();
            }
            return;
        }
        saveFormat = (SaveFormat)EditorGUILayout.EnumPopup("Export Format", saveFormat);

        saveResolution = (SaveResolution)EditorGUILayout.EnumPopup("Resolution", saveResolution);

        if (GUILayout.Button("Export"))
        {
            ExportWholeSelectionToSingle();
        }
    }

    public void TerrainMeshToFile(StreamWriter sw)
    {
        //string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");
        int w = terrain.heightmapWidth;
        int h = terrain.heightmapHeight;
        Vector3 meshScale = terrain.size;
        int tRes = (int)Mathf.Pow(2, (int)saveResolution);
        meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
        Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
        float[,] tData = terrain.GetHeights(0, 0, w, h);

        w = (w - 1) / tRes + 1;
        h = (h - 1) / tRes + 1;
        Vector3[] tVertices = new Vector3[w * h];
        Vector2[] tUV = new Vector2[w * h];

        int[] tPolys;

        if (saveFormat == SaveFormat.Triangles)
        {
            tPolys = new int[(w - 1) * (h - 1) * 6];
        }
        else
        {
            tPolys = new int[(w - 1) * (h - 1) * 4];
        }

        // Build vertices and UVs
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                tVertices[x * w + y] = Vector3.Scale(meshScale, new Vector3(y, tData[x * tRes, y * tRes], x)) + terrainPos;
                tUV[x * w + y] = Vector2.Scale(new Vector2(y * tRes,x * tRes), uvScale);
            }
        }

        int index = 0;
        if (saveFormat == SaveFormat.Triangles)
        {
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = (y * w) + x + 1;

                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }
        }
        else
        {
            // Build quad indices: 4 indices into vertex array for each quad
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output one quad
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }
        }

        // Export to .obj
        if (sw == null)
        {
            return;
        }
        
        try
        {

            sw.WriteLine("g Unity terrain");

            // Write vertices
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            counter = tCount = 0;
            totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / progressUpdateInterval;
            for (int i = 0; i < tVertices.Length; i++)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("v ", 20);
                // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
                // Which is important when you're exporting huge terrains.
                sb.Append(tVertices[i].x.ToString()).Append(" ").
                   Append(tVertices[i].y.ToString()).Append(" ").
                   Append(tVertices[i].z.ToString());
                sw.WriteLine(sb);
            }
            // Write UVs
            for (int i = 0; i < tUV.Length; i++)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("vt ", 22);
                sb.Append(tUV[i].x.ToString()).Append(" ").
                   Append(tUV[i].y.ToString());
                sw.WriteLine(sb);
            }
            if (saveFormat == SaveFormat.Triangles)
            {
                // Write triangles
                for (int i = 0; i < tPolys.Length; i += 3)
                {
                    UpdateProgress();
                    StringBuilder sb = new StringBuilder("f ", 43);
                    sb.Append(tPolys[i] + 1 + vertexOffset).Append("/").Append(tPolys[i] + 1 + vertexOffset).Append(" ").
                       Append(tPolys[i + 1] + 1 + vertexOffset).Append("/").Append(tPolys[i + 1] + 1 + vertexOffset).Append(" ").
                       Append(tPolys[i + 2] + 1 + vertexOffset).Append("/").Append(tPolys[i + 2] + 1 + vertexOffset);
                    sw.WriteLine(sb);
                }
            }
            else
            {
                // Write quads
                for (int i = 0; i < tPolys.Length; i += 4)
                {
                    UpdateProgress();
                    StringBuilder sb = new StringBuilder("f ", 57);
                    sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                       Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                       Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
                       Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
                    sw.WriteLine(sb);
                }
            }
        }
        catch (Exception err)
        {
            Debug.Log("Error saving file: " + err.Message);
        }

        sw.Close();

        terrain = null;
        EditorUtility.ClearProgressBar();
        EditorWindow.GetWindow<ExportTerrain>().Close();
    }

    void UpdateProgress()
    {
        if (counter++ == progressUpdateInterval)
        {
            counter = 0;
            EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
        }
    }

    static void ExportWholeSelectionToSingle()
    {
        if (!CreateTargetFolder())
            return;
        //string fileName = EditorUtility.SaveFilePanel("Export .obj file", targetFolder, "Terrain", "obj");
        GameObject[] selection = instance.passableObjs;

        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
            return;
        }

        int exportedObjects = 0;

        ArrayList mfList = new ArrayList();

        for (int i = 0; i < selection.Length; i++)
        {
            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

            for (int m = 0; m < meshfilter.Length; m++)
            {
                exportedObjects++;
                mfList.Add(meshfilter[m]);
            }
        }

        if (exportedObjects > 0)
        {
            MeshFilter[] mf = new MeshFilter[mfList.Count];

            for (int i = 0; i < mfList.Count; i++)
            {
                mf[i] = (MeshFilter)mfList[i];
            }

            string filename = EditorApplication.currentScene + "_" + exportedObjects;

            int stripIndex = filename.LastIndexOf('/');//FIXME: Should be Path.PathSeparator

            if (stripIndex >= 0)
                filename = filename.Substring(stripIndex + 1).Trim();

            MeshesToFile(mf, targetFolder, filename);


            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects to " + filename, "");
        }
        else
            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
    }

    private static void MeshesToFile(MeshFilter[] mf, string folder, string filename)
    {
        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
        {
           /* sw.Write("mtllib ./" + filename + ".mtl\n");*/

            for (int i = 0; i < mf.Length; i++)
            {
                sw.Write(MeshToString(mf[i], materialList));
            }
            instance.TerrainMeshToFile(sw);
        }

        //MaterialsToFile(materialList, folder, filename);
    }

    private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList)
    {
        Mesh m = mf.sharedMesh;
        //Material[] mats = mf.renderer.sharedMaterials;
        if (m == null || m.vertexCount == 0 )
        {
            return null;
        }
        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 lv in m.vertices)
        {
            Vector3 wv = mf.transform.TransformPoint(lv);

            //This is sort of ugly - inverting x-component since we're in
            //a different coordinate system than "everyone" is "used to".
            sb.Append(string.Format("v {0} {1} {2}\n", wv.x, wv.y, wv.z));
        }
        sb.Append("\n");

        foreach (Vector3 lv in m.normals)
        {
            Vector3 wv = mf.transform.localRotation * lv/*mf.transform.TransformDirection(lv)*/;

            sb.Append(string.Format("vn {0} {1} {2}\n", wv.x, wv.y, wv.z));
        }
        sb.Append("\n");

        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }

        for (int material = 0; material < m.subMeshCount; material++)
        {
//             sb.Append("\n");
//             sb.Append("usemtl ").Append(mats[material].name).Append("\n");
//             sb.Append("usemap ").Append(mats[material].name).Append("\n");

            //See if this material is already in the materiallist.
//             try
//             {
//                 ObjMaterial objMaterial = new ObjMaterial();
// 
//                 objMaterial.name = mats[material].name;
// 
//                 if (mats[material].mainTexture)
//                     objMaterial.textureName = AssetDatabase.GetAssetPath(mats[material].mainTexture);
//                 else
//                     objMaterial.textureName = null;
// 
//                 materialList.Add(objMaterial.name, objMaterial);
//             }
//             catch (ArgumentException)
//             {
//                 //Already in the dictionary
//             }


            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                //Because we inverted the x-component, we also needed to alter the triangle winding.
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
            }
        }

        vertexOffset += m.vertices.Length;
        normalOffset += m.normals.Length;
        uvOffset += m.uv.Length;

        return sb.ToString();
    }

    private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
    {
        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".mtl"))
        {
            foreach (KeyValuePair<string, ObjMaterial> kvp in materialList)
            {
                sw.Write("\n");
                sw.Write("newmtl {0}\n", kvp.Key);
                sw.Write("Ka  0.6 0.6 0.6\n");
                sw.Write("Kd  0.6 0.6 0.6\n");
                sw.Write("Ks  0.9 0.9 0.9\n");
                sw.Write("d  1.0\n");
                sw.Write("Ns  0.0\n");
                sw.Write("illum 2\n");

                if (kvp.Value.textureName != null)
                {
                    string destinationFile = kvp.Value.textureName;


                    int stripIndex = destinationFile.LastIndexOf('/');//FIXME: Should be Path.PathSeparator;

                    if (stripIndex >= 0)
                        destinationFile = destinationFile.Substring(stripIndex + 1).Trim();


                    string relativeFile = destinationFile;

                    destinationFile = folder + "/" + destinationFile;

                    Debug.Log("Copying texture from " + kvp.Value.textureName + " to " + destinationFile);

                    try
                    {
                        //Copy the source file
                        File.Copy(kvp.Value.textureName, destinationFile);
                    }
                    catch
                    {

                    }


                    sw.Write("map_Kd {0}", relativeFile);
                }

                sw.Write("\n\n\n");
            }
        }
    }

    private static Dictionary<string, ObjMaterial> PrepareFileWrite()
    {
        Clear();

        return new Dictionary<string, ObjMaterial>();
    }

    private static void Clear()
    {
        vertexOffset = 0;
        normalOffset = 0;
        uvOffset = 0;
    }

    private static bool CreateTargetFolder()
    {
        try
        {
            System.IO.Directory.CreateDirectory(targetFolder);
        }
        catch
        {
            EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
            return false;
        }

        return true;
    }
}