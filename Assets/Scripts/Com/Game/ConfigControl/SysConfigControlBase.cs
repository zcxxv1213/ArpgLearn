using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Com.Game.ConfigControl
{
    public interface ISysConfigControlBase
    {
        void Init(Dictionary<string, object> data);
        void Init(object data);
        void Init(IList list);
        void InitMS(Dictionary<string, MemoryStream> dic);
        void SetTableName(string tableName);
        string GetTableName();
        void InitAllData(bool value);
        int GetConfigDataCount();
        Type GetValueType();
        bool IsInit();
    }

    public class SysConfigControlBase<TKey, TValue> : ISysConfigControlBase where TValue : new()
    {
        private List<TKey> mKeys;
        private Dictionary<TKey, TValue> mDicData;
        private Dictionary<TKey, MemoryStream> mRawData;
        private int mCount;

        public bool IsInit()
        {
            return mDicData != null;
        }

        public List<TKey> GetKeys()
        {
            return mKeys;
        }

        public Dictionary<TKey, TValue> GetConfigDataDic()
        {
            ReadAllRawData();

            return mDicData;
        }

        public TValue GetConfigData(TKey key)
        {
            TValue data = default(TValue);

            MemoryStream ms = null;
            if (mRawData != null && mRawData.TryGetValue(key, out ms))
            {
                data = Deserialize(ms);
                mDicData[key] = data;
                mRawData.Remove(key);

                if (mRawData.Count == 0)
                {
                    mRawData = null;
                }
            }
            else
                mDicData.TryGetValue(key, out data);

            return data;
        }

        MethodInfo method;
        const string cDeserializer = "Deserializer";
        private TValue Deserialize(MemoryStream ms)
        {
            //BinaryFormatter formatter = new BinaryFormatter();
            //TValue value = (TValue)formatter.Deserialize(ms);
            TValue value = new TValue();

            if (method == null)
                method = typeof(TValue).GetMethod(cDeserializer, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            method.Invoke(value, new object[] { ms.ToArray(), 0 });

            ms.Dispose();

            return value;
        }

        public Type GetValueType()
        {
            return typeof(TValue);
        }

        public int GetConfigDataCount()
        {
            return mCount;
        }

        private void ReadAllRawData()
        {
            if (mRawData != null && mRawData.Count > 0)
            {
                foreach (KeyValuePair<TKey, MemoryStream> kvp in mRawData)
                {
                    TValue data = Deserialize(kvp.Value);
                    mDicData[kvp.Key] = data;
                }

                mRawData.Clear();
                mRawData = null;
            }
        }

        public List<TValue> GetConfigDataList()
        {
            ReadAllRawData();

            return mDicData.Values.ToList();
        }

        string mTableName;
        public void SetTableName(string tableName)
        {
            mTableName = tableName;
        }

        bool mInitAllData;
        public void InitAllData(bool value)
        {
            mInitAllData = value;
        }

        public string GetTableName()
        {
            return mTableName;
        }

        public void InitMS(Dictionary<string, MemoryStream> data)
        {
            mCount = data.Count;
            mDicData = new Dictionary<TKey, TValue>();
            mKeys = new List<TKey>();

            foreach (KeyValuePair<string, MemoryStream> kvp in data)
            {
                TKey key = (TKey)Convert.ChangeType(kvp.Key, typeof(TKey));
                mKeys.Add(key);

                if (mInitAllData)
                {
                    TValue value = Deserialize(kvp.Value);
                    mDicData[key] = value;
                    PreInit(key, value);
                }
                else
                {
                    if (mRawData == null)
                        mRawData = new Dictionary<TKey, MemoryStream>();

                    mRawData[key] = kvp.Value;
                }

            }

            OnInitComplete();
        }

        public void Init(IList list)
        {
            mDicData = new Dictionary<TKey, TValue>();

            FieldInfo f = GetValueType().GetField("unikey");

            for (int i = 0, count = list.Count; i < count; i++)
            {
                object obj = list[i];
                TKey key = (TKey)Convert.ChangeType((string)f.GetValue(obj), typeof(TKey));
                TValue value = (TValue)obj;
                mDicData[key] = value;

                PreInit(key, value);
            }

            OnInitComplete();
        }

        public void Init(object data)
        {
            Dictionary<string, object> dicData = new Dictionary<string, object>();
            List<TValue> listData = data as List<TValue>;

            FieldInfo f = GetValueType().GetField("unikey");
            foreach (var e in listData)
            {
                dicData.Add((string)f.GetValue(e), e);
            }

            Init(dicData);
        }

        public void Init(Dictionary<string, object> data)
        {
            mCount = data.Count;
            mDicData = new Dictionary<TKey, TValue>();
            mKeys = new List<TKey>();

            foreach (KeyValuePair<string, object> kvp in data)
            {
                TKey key = (TKey)Convert.ChangeType(kvp.Key, typeof(TKey));
                TValue value = (TValue)kvp.Value;
                mDicData[key] = value;
                mKeys.Add(key);

                PreInit(key, value);
            }
            OnInitComplete();
        }

        //提供接口对数据进行预处理，比如以数据某些字段为key去索引数据等。
        protected virtual void PreInit(TKey key, TValue value)
        {

        }

        protected virtual void OnInitComplete()
        {

        }
    }
}
