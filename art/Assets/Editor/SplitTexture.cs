using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    class SplitTexture : EditorWindow
    {
        [MenuItem("Assets/SplitTexture/Split", false, 0)]
        static public void Split()
        {
            string[] assetGUIDs = Selection.assetGUIDs;
            if (assetGUIDs.Length <= 0)
                return;

            for (int k = 0, len = assetGUIDs.Length; k < len; k++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[k]);

                bool updateReadable = false;
                TextureImporter TextureI = AssetImporter.GetAtPath(path) as TextureImporter;
                if (TextureI == null)
                    continue;

                if (TextureI.isReadable == false)
                {
                    updateReadable = true;
                    TextureI.isReadable = true;
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }

                Texture2D tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                if (tex == null)
                    continue;

                Color32[] colors = tex.GetPixels32();
                int count = colors.Length;
                Color[] rgbColors = new Color[count];
                Color32[] alphaColors = new Color32[count];

                for (int i = 0; i < count; i++)
                {
                    Color32 rawColor = colors[i];
                    rgbColors[i] = rawColor;
                    alphaColors[i] = new Color32(rawColor.a, rawColor.a, rawColor.a, 0);
                }

                string directory = Path.GetDirectoryName(path) + "/";

                Texture2D rgbTex = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, true);
                rgbTex.SetPixels(rgbColors);
                byte[] rgbBytes = rgbTex.EncodeToPNG();
                SaveFile(directory + Path.GetFileNameWithoutExtension(tex.name) + "_C.png", rgbBytes);

                Texture2D alphaTex = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, true);
                alphaTex.SetPixels32(alphaColors);
                byte[] aBytes = alphaTex.EncodeToPNG();
                SaveFile(directory + Path.GetFileNameWithoutExtension(tex.name) + "_A.png", aBytes);

                if (updateReadable)
                {
                    TextureI = AssetImporter.GetAtPath(path) as TextureImporter;
                    TextureI.isReadable = false;
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }

                Debug.Log(string.Format("Split {0} success", tex.name));
            }

            AssetDatabase.Refresh();
        }

        public static void SaveFile(string path, byte[] bytes)
        {
            string directory = Path.GetDirectoryName(path);

            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                try
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
                catch (Exception exception)
                {
                    Debug.Log("SaveFile:" + exception.StackTrace);
                }
            }
        }
    }
}
