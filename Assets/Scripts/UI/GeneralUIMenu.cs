//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

#if WINDOWS_UWP
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Windows.ApplicationModel.Core;
#else
using GoogleARCore.Examples.HelloAR;
using GoogleARCore.Examples.Common;
#endif

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
        
		// KEep track of the scanned grid status.
		private bool flagGridVisible = true;
        
		// Events to be raised on clicks:
		// - exit application
		public void OnExit() {
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#elif WINDOWS_UWP
			CoreApplication.Exit();
			#else // UNITY_ANDROID
			Application.Quit();
			#endif
		}
		// - Destroy CPlane
		public void OnDestroyCPlane() {
			//TODO not actually delete c plane but start placing it (put infron tf camera and activate placable)
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
            #if WINDOWS_UWP
			ObjectManager.instance.cPlane.GetComponent<Placeable>().OnTap();
            #else
            ObjectManager.instance.cPlane.SetActive(false);
            //DestroyImmediate(ObjectManager.instance.cPlane);
			//Resources.UnloadUnusedAssets();
			#endif
		}
		// - Toggle AR Core Grid
		public void OnTogglePointsAndGrids() {
			List<MeshRenderer> renderers = new List<MeshRenderer>();;
			this.flagGridVisible = !this.flagGridVisible;
			#if WINDOWS_UWP
			// Microsoft Windows MRTK
			// Cast the Spatial Awareness system to IMixedRealityDataProviderAccess to get an Observer
			var access = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
			// Get the first Mesh Observer available, generally we have only one registered
			var observers = access.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
			// Loop through all known Meshes
			foreach (var observer in observers) {
				foreach (SpatialAwarenessMeshObject meshObject in observer.Meshes.Values) {
					renderers.Add(meshObject.Renderer);
				}
			}
			if (this.flagGridVisible) {
				// Resume Mesh Observation from all Observers
				CoreServices.SpatialAwarenessSystem.ResumeObservers();
			} else {
				// Suspend Mesh Observation from all Observers
				CoreServices.SpatialAwarenessSystem.SuspendObservers();
			}
			#else
			// Android ARkit
			PointcloudVisualizer[] visualizers = FindObjectsOfType<PointcloudVisualizer>();
			foreach (PointcloudVisualizer visualizer in visualizers)
				renderers.Add(visualizer.meshRenderer);
			DetectedPlaneVisualizer.flagVisible = this.flagGridVisible;
			#endif
			foreach (MeshRenderer renderer in renderers)
				renderer.enabled = this.flagGridVisible;
		}
		// - Destroy Objects
		public void OnDestroyObjects() {
			ObjectManager.instance.gameObject.GetComponent<MeshProcessor>().DeleteMeshes(SourceType.TCP);
			ObjectManager.instance.gameObject.GetComponent<MeshProcessor>().DeleteMeshes(SourceType.UDP);
			ObjectManager.instance.gameObject.GetComponent<TagProcessor>().DeleteTags();
			ObjectManager.instance.gameObject.GetComponent<RobotProcessor>().DeleteRobots();
			ObjectManager.instance.gameObject.GetComponent<Point3DProcessor>().DeletePoints();
		}
		public void OnAdd3DPoint() {
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
            
			ObjectManager.instance.gameObject.GetComponent<Point3DProcessor>().AddPoint();
		}
	}
}