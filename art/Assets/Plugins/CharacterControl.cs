using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Plugins
{
    [ExecuteInEditMode]
    class CharacterControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent navMeshAgent;
        public CameraControl mCameraControl;

        void Awake()
        {
            this.navMeshAgent = this.transform.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (this.navMeshAgent == null)
            {
                this.navMeshAgent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
                this.navMeshAgent.walkableMask = 1;
            }

            this.navMeshAgent.angularSpeed = 720;

            mCameraControl = GetCameraControl();
            mCameraControl.followTarget = this.transform;
			
			this.transform.localScale = new Vector3(1.25f,1.25f,1.25f);
        }

        public CameraControl GetCameraControl()
        {
            Camera mainCamera = Camera.main;

            CameraControl cameraControl = mainCamera.GetComponent<CameraControl>();
            if (cameraControl == null)
            {
                cameraControl = mainCamera.gameObject.AddComponent<CameraControl>();
                SetCameraParam(cameraControl);
            }

            return cameraControl;
        }

        public void SetCameraParam(CameraControl control)
        {
            control.SetFieldOfView(23.5f);
            control.offset = new Vector3(0, 14f, -21f);
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                this.transform.position += this.transform.forward * this.navMeshAgent.speed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                this.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                this.transform.position += this.transform.forward * this.navMeshAgent.speed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                this.transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
                this.transform.position += this.transform.forward * this.navMeshAgent.speed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                this.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                this.transform.position += this.transform.forward * this.navMeshAgent.speed * Time.deltaTime;
            }
        }

        public void OnDestroy()
        {
            if (mCameraControl != null)
            {
                GameObject.DestroyImmediate(mCameraControl);
            }
        }


    }
}
