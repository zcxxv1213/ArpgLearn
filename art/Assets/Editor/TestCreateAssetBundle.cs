using UnityEngine;
using System.Collections;
using UnityEditor;
using Assets;

public class TestCreateAssetBundle : Editor
{

    //单独打包
    //[MenuItem("Asset/Build AssetBundles Main")]
    static void ExportAssetBundlesMain()
    {
        //获取选择的目录
        //相同的资源尽可能打包在一起，他们公用一套资源
        //不相同的模型尽量分开打包

        //SelectionMode.DeepAssets
        /*这个选择模式意味着如果选择中包含多个文件，那马他将包含这个文件视图中的所有资源
         */

        Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        foreach(Object obj in selects)
        {
            //这里建立一个本地测试，
            //注意本地测试可以是任意类型的文件，但是到了移动平台只能读取StreamingAssets里面的
            //StreamingAssets是只读路径，不能写入

            string targetPath = Application.dataPath + "/StreamingAssets/" + obj.name + ".assetbundle";

            //if (BuildPipeline.BuildAssetBundle(obj, null, targetPath, BuildAssetBundleOptions.CollectDependencies))
            //{
            //    Debug.Log(obj.name + "is packed successfully!");
            //}
            //else
            //{
            //    Debug.Log(obj.name + "is packed faily!");
            //}

            //刷新编辑器，不刷新的话打包后的资源是不能马上看到的
            AssetDatabase.Refresh();

        }

	}

    //打包在一起
    //[MenuItem("Asset/Build AssetBundles All")]
    static void ExportAssetBundlesAll()
    {

        Object[] selects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        string targetPath = Application.dataPath + "/StreamingAssets/Together.assetbundle";

       /* if(BuildPipeline.BuildAssetBundle(null, selects, targetPath, BuildAssetBundleOptions.CollectDependencies))
        {
            Debug.Log("packed successfully");
        }
        else
        {
            Debug.Log("pached faily");
        }*/

        AssetDatabase.Refresh();

    }

    //加载
    //不同平台下StreamingAssets的路径不同，这里需要注意一下
    public static readonly string m_PathURL =
#if UNITY_ANDROID
        "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE
        Application.dataPath + "/Raw/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
 "file://" + Application.dataPath + "/StreamingAssets/";
#else
      string.Empty;
#endif


}
