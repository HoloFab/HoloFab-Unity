//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	public class Placeable : MonoBehaviour {
		public string scanMeshLayerName = "Spatial Awareness";
		public float offsetDistance = 4f;
		public bool flagSelected = true;
        
		private bool flagTapped = false;
        
		private string sourceName = "Placeable";
        
		// public int historySize = 10;
		// private List<Vector3> historyPosition = new List<Vector3>();
		// private List<Vector3> historyNormal = new List<Vector3>();
        
		void Update(){
            #if WINDOWS_UWP
            if (this.flagSelected || this.flagTapped) {
				Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit)) {
					if (hit.collider.gameObject.layer == LayerMask.NameToLayer(this.scanMeshLayerName)) {
						Place(hit.point, hit.normal);
						// if tapped - deselect
						if (this.flagTapped) this.flagSelected = false;
						this.flagTapped = false;
						return;
					}
				}
				Place(Camera.main.transform.position + Camera.main.transform.forward * this.offsetDistance, Vector3.up);
				// If tried placing not on grid - force placement back on.
				if (this.flagTapped) this.flagSelected = true;
				this.flagTapped = false;
			}
            #endif
		}
        
		public void OnTap(){
            #if WINDOWS_UWP
			// if (!this.flagSelected) {
			// 	this.historyPosition = new List<Vector3>();
			// 	this.historyNormal = new List<Vector3>();
			// }
			this.flagTapped = true;
			this.flagSelected = !this.flagSelected;
            
			#if DEBUG
			DebugUtilities.UniversalDebug(sourceName, "Tapped: New State: " + this.flagSelected);
			#endif
            #endif
		}
        
		private void Place(Vector3 position, Vector3 normal){
            #if WINDOWS_UWP			
            // this.historyPosition.Add(position);
			// if (this.historyPosition.Count > this.historySize)
			// 	this.historyPosition.RemoveAt(0);
			// Vector3 positionAverage = Vector3.zero;
			// foreach (Vector3 item in this.historyPosition)
			// 	positionAverage += item;
			// positionAverage /= this.historyPosition.Count;
			//
			// // this.historyNormal.Add(normal);
			// // if (this.historyNormal.Count > this.historySize)
			// // 	this.historyNormal.RemoveAt(0);
			// // Vector3 normalAverage = Vector3.zero;
			// // foreach (Vector3 item in this.historyNormal)
			// // 	normalAverage += item;
			// // normalAverage /= this.historyNormal.Count;
			//
			// transform.position = positionAverage;
            
			transform.position = position;
			// transform.localRotation = Quaternion.FromToRotation(transform.up, normal);
            #endif
		}
	}
}