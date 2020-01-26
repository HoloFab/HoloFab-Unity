//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using GoogleARCore.Examples.HelloAR;
using GoogleARCore.Examples.Common;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Unity Component Controlling Addition and Removal of Generated Objects.
	public class GeneralUIMenu : MonoBehaviour {
		[Header("Variables Set From Scene.")]
		[Tooltip("Button to exit application.")]
		public Button buttonExit;
		[Tooltip("Button to Destroy CPlane.")]
		public Button buttonDestroyCplane;
		[Tooltip("Button to Toggle Grid.")]
		public Button buttonToggleGrid;
		[Tooltip("Button to Delete Objects.")]
		public Button buttonObjects;

        public GameObject goP3D;

        // - CPlane object.
        private string tagCPlane = "CPlane";
        private GameObject cPlane;


        bool flagGridVisible = true;
        		
		// Events to be raised on clicks:
		// - exit application
		public void OnExit() {
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
		// - Destroy CPlane
		public void OnDestroyCPlane() {
			GameObject cPlane = GameObject.FindGameObjectWithTag("CPlane");
			DestroyImmediate(cPlane);
			Resources.UnloadUnusedAssets();
		}
		// - Toggle AR Core Grid
		public void OnTogglePointsAndGrids() {
			this.flagGridVisible = !this.flagGridVisible;
            
			MeshRenderer renderer = FindObjectOfType<PointcloudVisualizer>().meshRenderer;
			renderer.enabled = this.flagGridVisible;
			DetectedPlaneVisualizer.flagVisible = this.flagGridVisible;
		}
		// - Destroy Objects
		public void OnDestroyObjects() {
			ObjectManager.instance.gameObject.GetComponent<TagProcessor>().DeleteTags();
			ObjectManager.instance.gameObject.GetComponent<MeshProcessor>().DeleteMeshes(SourceType.TCP);
			ObjectManager.instance.gameObject.GetComponent<MeshProcessor>().DeleteMeshes(SourceType.UDP);
			ObjectManager.instance.gameObject.GetComponent<RobotProcessor>().DeleteRobots();			
		}
        public void OnAdd3DPoint()
        {
            cPlane = GameObject.FindGameObjectWithTag(this.tagCPlane);
            GameObject p3D = Instantiate(goP3D, Camera.main.transform.position + Camera.main.transform.forward, this.cPlane.transform.rotation, this.cPlane.transform);
        }
    }
}