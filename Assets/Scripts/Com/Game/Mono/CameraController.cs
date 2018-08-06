using Com.Game.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Com.Game.Mono
{
    public class CameraController : Singleton<CameraController>
    {
        Camera mCamera;
        public Camera GetCamera()
        {
            if (mCamera != null)
            {
                return mCamera;
            }
            else
            {
                mCamera = Camera.main;
                if (mCamera == null)
                    return null;
                else
                    return mCamera;
            }
        }
        private void SetCameraParamsWhenEnterBattle()
        {
            if (this.GetCamera())
            {
                mCamera.transform.position = Vector3.zero;
                mCamera.transform.eulerAngles = new Vector3(30, 0, 0);
                mCamera.fieldOfView = 60;
            }
        }
        public Ray ScreenPointToRay(Vector3 pos)
        {
            return mCamera.ScreenPointToRay(pos);
        }
        public Vector3 WorldToScreenPoint(Vector3 pos)
        {
            return mCamera.WorldToScreenPoint(pos);
        }
        public Vector3 ScreenToWorldPoint(Vector3 pos)
        {
            return mCamera.ScreenToWorldPoint(pos);
        }
        public void OnEnterBattleScene()
        {
            this.SetCameraParamsWhenEnterBattle(); 
        }
        public void OnExitBattleScene()
        {
            this.mCamera = null;
        }
    }
}