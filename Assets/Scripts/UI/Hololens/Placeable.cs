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
		public float offsetDistance = 2f;
		public bool flagSelected = true;
        
		private bool flagTapped = false;
        
		void Update(){
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
		}
        
		public void OnTap(){
			this.flagTapped = true;
			this.flagSelected = !this.flagSelected;
		}
        
		private void Place(Vector3 position, Vector3 normal){
			transform.position = position;
			// transform.localRotation = Quaternion.FromToRotation(transform.up, normal);
		}
	}
}