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
        
		// Local Variables:
		// - flag grid visible
		bool flagGridVisible = true;
        
		void Start() {
			// buttonExit.onClick.AddListener(OnExit);
			// buttonDestroyCplane.onClick.AddListener(OnDestroyCPlane);
			// buttonToggleGrid.onClick.AddListener(OnTogglePointsAndGrids);
			// buttonObjects.onClick.AddListener(OnDestroyObjects);
		}
		//////////////////////////////////////////////////////////////////////////
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
			// Reset CPlane setting for AR core
			HoloFabARController.flagCPlaneSet = false;
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
			//TagProcessor.DeleteTags();
			//TagProcessor.DeleteTags();
			//TagProcessor.DeleteTags();
			GameObject[] go = GameObject.FindGameObjectsWithTag("ReceivedMesh");
			GameObject[] goPlus = GameObject.FindGameObjectsWithTag("ReceivedMeshPlus");
			GameObject[] goL = GameObject.FindGameObjectsWithTag("labels");
            
			for (int i = 0; i < go.Length; i++) {
				Destroy(go[i]);
			}
			for (int i = 0; i < goPlus.Length; i++) {
				Destroy(goPlus[i]);
			}
			for (int i = 0; i < goL.Length; i++) {
				Destroy(goL[i]);
			}
			Resources.UnloadUnusedAssets();
		}
	}
}