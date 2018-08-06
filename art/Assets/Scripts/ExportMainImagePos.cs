using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExportMainImagePos : MonoBehaviour {
    private Transform mBigMapExportNode;
    private Transform mCitiesExportNode;
    private EventSystem es;
    public void CreatXmlMapConfig()
    {
        string text = "local ConfigBigMap = {" + "\r\n ";
        int num = 1;
        var BigMapExportObj = GameObject.FindGameObjectWithTag("BigMapExportTag");
        if (BigMapExportObj)
        {
            mBigMapExportNode = BigMapExportObj.transform;
            if (mBigMapExportNode)
            {
                GameObject[] mChildGameObjects = new GameObject[mBigMapExportNode.transform.childCount];
                for (int i = 0; i < mBigMapExportNode.transform.childCount; i++)
                {
                    mChildGameObjects[i] = mBigMapExportNode.GetChild(i).gameObject;
                    mChildGameObjects[i].AddComponent<AddBoxColider>();
                }
                XElement xElement = new XElement("BigMapNode");
                foreach (var v in mChildGameObjects)
                {
                    bool IfLeft = false;
                    bool IfRight = false;
                    bool IfUp = false;
                    bool IfDown = false;
                    //CheckRight
                    if (Physics2D.Raycast(new Vector2(v.transform.position.x + (v.GetComponent<RectTransform>().sizeDelta.x / 2 + 1), v.transform.position.y), transform.forward))
                        IfRight = true;
                    //CheckLeft
                    if (Physics2D.Raycast(new Vector2(v.transform.position.x - (v.GetComponent<RectTransform>().sizeDelta.x / 2 + 1), v.transform.position.y), transform.forward))
                        IfLeft = true;
                    //CheckUp
                    if (Physics2D.Raycast(new Vector2(v.transform.position.x, v.transform.position.y + (v.GetComponent<RectTransform>().sizeDelta.y / 2 + 1)), transform.forward))
                        IfUp = true;
                    //CheckDown
                    if (Physics2D.Raycast(new Vector2(v.transform.position.x, v.transform.position.y - (v.GetComponent<RectTransform>().sizeDelta.y / 2 + 1)), transform.forward))
                        IfDown = true;


                    var imageName = v.GetComponent<Image>().sprite.name;
                    if (!IfLeft && !IfDown && IfRight && IfUp)
                    {
                        xElement.Add(new XElement(("Node"),
                            new XElement("posX", v.transform.localPosition.x),
                            new XElement("posY", v.transform.localPosition.y),
                            new XElement("sizeX", v.transform.GetComponent<RectTransform>().sizeDelta.x),
                            new XElement("sizeY", v.transform.GetComponent<RectTransform>().sizeDelta.y),
                            new XElement("part", "DownLeft"),
                             new XElement("image", imageName))
                            );
                    }
                    if (!IfLeft && !IfUp && IfRight && IfDown)
                    {
                        xElement.Add(new XElement(("Node"),
                            new XElement("posX", v.transform.localPosition.x),
                            new XElement("posY", v.transform.localPosition.y),
                            new XElement("sizeX", v.transform.GetComponent<RectTransform>().sizeDelta.x),
                            new XElement("sizeY", v.transform.GetComponent<RectTransform>().sizeDelta.y),
                            new XElement("part", "UpLeft"),
                             new XElement("image", imageName))
                            );
                    }
                    if (IfLeft && IfUp && !IfRight && !IfDown)
                    {
                        xElement.Add(new XElement(("Node"),
                            new XElement("posX", v.transform.localPosition.x),
                            new XElement("posY", v.transform.localPosition.y),
                            new XElement("sizeX", v.transform.GetComponent<RectTransform>().sizeDelta.x),
                            new XElement("sizeY", v.transform.GetComponent<RectTransform>().sizeDelta.y),
                            new XElement("part", "DownRight"),
                             new XElement("image", imageName))
                            );
                    }
                    if (IfLeft && !IfUp && !IfRight && IfDown)
                    {
                        xElement.Add(new XElement(("Node"),
                            new XElement("posX", v.transform.localPosition.x),
                            new XElement("posY", v.transform.localPosition.y),
                            new XElement("sizeX", v.transform.GetComponent<RectTransform>().sizeDelta.x),
                            new XElement("sizeY", v.transform.GetComponent<RectTransform>().sizeDelta.y),
                            new XElement("part", "UpRight"),
                             new XElement("image", imageName))
                            );
                    }
                    if (!IfLeft && IfDown && IfRight && IfUp)
                    {
                        xElement.Add(new XElement(("Node"),
                            new XElement("posX", v.transform.localPosition.x),
                            new XElement("posY", v.transform.localPosition.y),
                            new XElement("sizeX", v.transform.GetComponent<RectTransform>().sizeDelta.x),
                            new XElement("sizeY", v.transform.GetComponent<RectTransform>().sizeDelta.y),
                            new XElement("part", "Left"),
                             new XElement("image", imageName))
                            );
                    }
                    if (IfLeft && !IfDown && IfRight && IfUp)
                    {
                        xElement.Add(new XElement(("Node"),
                               new XElement("posX", v.transform.localPosition.x),
                               new XElement("posY", v.transform.localPosition.y),
                               new XElement("sizeX", v.transform.GetComponent<RectTransform>().sizeDelta.x),
                               new XElement("sizeY", v.transform.GetComponent<RectTransform>().sizeDelta.y),
                               new XElement("part", "Down"),
                                new XElement("image", imageName))
                               );
                    }
                    if (IfLeft && IfDown && !IfRight && IfUp)
                    {
                        xElement.Add(new XElement(("Node"),
                              new XElement("posX", v.transform.localPosition.x),
                              new XElement("posY", v.transform.localPosition.y),
                              new XElement("sizeX", v.transform.GetComponent<RectTransform>().sizeDelta.x),
                              new XElement("sizeY", v.transform.GetComponent<RectTransform>().sizeDelta.y),
                              new XElement("part", "Right"),
                               new XElement("image", imageName))
                              );
                    }
                    if (IfLeft && IfDown && IfRight && !IfUp)
                    {
                        xElement.Add(new XElement(("Node"),
                              new XElement("posX", v.transform.localPosition.x),
                              new XElement("posY", v.transform.localPosition.y),
                              new XElement("sizeX", v.transform.GetComponent<RectTransform>().sizeDelta.x),
                              new XElement("sizeY", v.transform.GetComponent<RectTransform>().sizeDelta.y),
                              new XElement("part", "Up"),
                               new XElement("image", imageName))
                              );
                    }
                    if (IfLeft && IfDown && IfRight && IfUp)
                    {
                        xElement.Add(new XElement(("Node"),
                              new XElement("posX", v.transform.localPosition.x),
                              new XElement("posY", v.transform.localPosition.y),
                              new XElement("sizeX", v.transform.GetComponent<RectTransform>().sizeDelta.x),
                              new XElement("sizeY", v.transform.GetComponent<RectTransform>().sizeDelta.y),
                              new XElement("part", "Middle"),
                               new XElement("image", imageName))
                              );
                    }
                    num = num + 1;
                }
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = new UTF8Encoding(false);
                settings.Indent = true;
                DirectoryInfo info = new DirectoryInfo(System.Environment.CurrentDirectory);
                XmlWriter xw = XmlWriter.Create(info.Parent.Parent.FullName + @"\client\art\Assets\MapConfigFile" + "/" + "ConfigBigMap" + ".xml", settings);
                xElement.Save(xw);
                //写入文件
                xw.Flush();
                xw.Close();
            }
        }
        
    }
    public void ExportLuaMapConfig()
    {
        string text = "local ConfigBigMap = {" + "\r\n ";
        string text2 = "local ConfigCitiesMap = {" + "\r\n ";
        int num2 = 1; 
        int num = 1;
        var BigMapExportObj  = GameObject.FindGameObjectWithTag("BigMapExportTag");
        if (BigMapExportObj)
        {
            mBigMapExportNode = BigMapExportObj.transform;
            if (mBigMapExportNode)
            {
                GameObject[] mChildGameObjects = new GameObject[mBigMapExportNode.transform.childCount];
                for (int i = 0; i < mBigMapExportNode.transform.childCount; i++)
                {
                    mChildGameObjects[i] = mBigMapExportNode.GetChild(i).gameObject;
                    mChildGameObjects[i].AddComponent<AddBoxColider>();
                }
                foreach (var v in mChildGameObjects)
                {
                    bool IfLeft = false;
                    bool IfRight = false;
                    bool IfUp = false;
                    bool IfDown = false;
                    //CheckRight
                    if (Physics2D.Raycast(new Vector2(v.transform.position.x + (v.GetComponent<RectTransform>().sizeDelta.x / 2 + 1), v.transform.position.y), transform.forward))
                        IfRight = true;
                    //CheckLeft
                    if (Physics2D.Raycast(new Vector2(v.transform.position.x - (v.GetComponent<RectTransform>().sizeDelta.x / 2 + 1), v.transform.position.y), transform.forward))
                        IfLeft = true;
                    //CheckUp
                    if (Physics2D.Raycast(new Vector2(v.transform.position.x, v.transform.position.y + (v.GetComponent<RectTransform>().sizeDelta.y / 2 + 1)), transform.forward))
                        IfUp = true;
                    //CheckDown
                    if (Physics2D.Raycast(new Vector2(v.transform.position.x, v.transform.position.y - (v.GetComponent<RectTransform>().sizeDelta.y / 2 + 1)), transform.forward))
                        IfDown = true;


                    var imageName = v.GetComponent<Image>().sprite.name;
                    if (!IfLeft && !IfDown && IfRight && IfUp)
                    {
                        text = BuildText(text, v.transform, num, "DownLeft", imageName);
                    }
                    if (!IfLeft && !IfUp && IfRight && IfDown)
                    {
                        text = BuildText(text, v.transform, num, "UpLeft", imageName);
                    }
                    if (IfLeft && IfUp && !IfRight && !IfDown)
                    {
                        text = BuildText(text, v.transform, num, "DownRight", imageName);
                    }
                    if (IfLeft && !IfUp && !IfRight && IfDown)
                    {
                        text = BuildText(text, v.transform, num, "UpRight", imageName);
                    }
                    if (!IfLeft && IfDown && IfRight && IfUp)
                    {
                        text = BuildText(text, v.transform, num, "Left", imageName);
                    }
                    if (IfLeft && !IfDown && IfRight && IfUp)
                    {
                        text = BuildText(text, v.transform, num, "Down", imageName);
                    }
                    if (IfLeft && IfDown && !IfRight && IfUp)
                    {
                        text = BuildText(text, v.transform, num, "Right", imageName);
                    }
                    if (IfLeft && IfDown && IfRight && !IfUp)
                    {
                        text = BuildText(text, v.transform, num, "Up", imageName);
                    }
                    if (IfLeft && IfDown && IfRight && IfUp)
                    {
                        text = BuildText(text, v.transform, num, "Middle", imageName);
                    }
                    num = num + 1;
                }
                text = text + "            }" + "\r\n ";
                text = text + "return ConfigBigMap";
                DirectoryInfo info = new DirectoryInfo(System.Environment.CurrentDirectory);
                FileStream luaFile = new FileStream(info.Parent.Parent.FullName + @"\client\art\Assets\MapConfigFile" + "/" + "ConfigBigMap" + ".lua", FileMode.Create);
                Encoding encoder = Encoding.UTF8;
                byte[] luabytes = encoder.GetBytes(text);
                luaFile.Write(luabytes, 0, luabytes.Length);
                luaFile.Close();
                Debug.Log("Export BigMap Config Success");
            }
        }
        var CitiesExportNode = GameObject.FindGameObjectWithTag("CitiesExportTag");
        if (CitiesExportNode)
        {
            mCitiesExportNode = CitiesExportNode.transform;
            if (mCitiesExportNode)
            {
                GameObject[] mChildGameObjects = new GameObject[mCitiesExportNode.transform.childCount];
                for (int i = 0; i < mCitiesExportNode.transform.childCount; i++)
                {
                    mChildGameObjects[i] = mCitiesExportNode.GetChild(i).gameObject;
                    mChildGameObjects[i].AddComponent<AddBoxColider>();
                }
                foreach (var v in mChildGameObjects)
                {
                    var imageName = v.GetComponent<Image>().sprite.name;
                    text2 = BuildCitiesText(text2, v.transform, num2, imageName);
                    num2 = num2 + 1;
                }
                text2 = text2 + "            }" + "\r\n ";
                text2 = text2 + "return ConfigCitiesMap";
                DirectoryInfo info = new DirectoryInfo(System.Environment.CurrentDirectory);
                FileStream luaFile = new FileStream(info.Parent.Parent.FullName + @"\client\art\Assets\MapConfigFile" + "/" + "ConfigCitiesMap" + ".lua", FileMode.Create);
                Encoding encoder = Encoding.UTF8;
                byte[] luabytes = encoder.GetBytes(text2);
                luaFile.Write(luabytes, 0, luabytes.Length);
                luaFile.Close();
                Debug.Log("Export CitesMap Config Success");
            }
        }
    }
    private string BuildText(string text, Transform mTransform, int mNum, string mPart, string ImageName)
    {
        text = text + "              [" + mNum + "]" + " = " + "{" + "\r\n ";
        text = text + "                      sizeX" + " = " + mTransform.GetComponent<RectTransform>().sizeDelta.x + "," + "\r\n ";
        text = text + "                      sizeY" + " = " + mTransform.GetComponent<RectTransform>().sizeDelta.y + "," + "\r\n ";
        text = text + "                      posX" + " = " + mTransform.localPosition.x + "," + "\r\n ";
        text = text + "                      posY" + " = " + mTransform.localPosition.y + "," + "\r\n ";
        text = text + "                      part" + " = " + "\"" + mPart + "\"" + "," + "\r\n ";
        text = text + "                      image" + " = " + "\"" + ImageName + "\"" + "," + "\r\n ";
        text = text + "                   }," + "\r\n ";
        return text;
    }
    private string BuildCitiesText(string text, Transform mTransform, int mNum, string ImageName)
    {
        text = text + "              [" + mNum + "]" + " = " + "{" + "\r\n ";
        text = text + "                      sizeX" + " = " + mTransform.GetComponent<RectTransform>().sizeDelta.x + "," + "\r\n ";
        text = text + "                      sizeY" + " = " + mTransform.GetComponent<RectTransform>().sizeDelta.y + "," + "\r\n ";
        text = text + "                      posX" + " = " + mTransform.localPosition.x + "," + "\r\n ";
        text = text + "                      posY" + " = " + mTransform.localPosition.y + "," + "\r\n ";
        text = text + "                      image" + " = " + "\"" + ImageName + "\"" + "," + "\r\n ";
        text = text + "                   }," + "\r\n ";
        return text;
    }
}
