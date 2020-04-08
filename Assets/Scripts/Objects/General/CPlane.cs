//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// A component responsible for CPlane transformation control.
	// TODO:
	// - Check if hit normal helped with rotations
	// - Check if tranformation updates are working better
	public class CPlane : MonoBehaviour {
		GameObject gObj = null;
		Plane objPlane;
		Vector3 m0;
		Vector3 r0;
		bool rotationMode;
		bool zMove, xyMove;
		float dist;
		Transform target;
		GameObject Z;
		public Transform camTransform;
		bool open;
        
		// Unity Functions.
		void Update() {
			Ray mouseRay = UnityUtilities.GenerateMouseRay();
			RaycastHit hit;
            
			if (Input.GetMouseButtonDown(0)) {
				if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit)) {
					gObj = hit.transform.gameObject;
					m0 = hit.transform.position - hit.point;
					dist = hit.transform.position.y - hit.point.y;
                    
					// Debug.Log(gObj.tag);
                    
					if (gObj.tag == "CPlaneXY") {
						gObj = gObj.transform.parent.gameObject;
						objPlane = new Plane(gObj.transform.up, gObj.transform.position);
						xyMove = true;
						rotationMode = false;
						zMove = false;
					} else if (gObj.tag == "CPlaneXYR") {
						gObj = gObj.transform.parent.gameObject;
						objPlane = new Plane(gObj.transform.up, gObj.transform.position);
						xyMove = false;
						rotationMode = true;
						zMove = false;
                        
						Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
						if (objPlane.Raycast(mRay, out float rayDistance)) {
							r0 = mRay.GetPoint(rayDistance) - gObj.transform.position;
						}
					} else if (gObj.tag == "CPlaneZ") {
						gObj = gObj.transform.parent.gameObject;
						objPlane = new Plane(Camera.main.transform.forward, gObj.transform.position);
						xyMove = false;
						rotationMode = false;
						zMove = true;
					}
					// else if (gObj.tag == "Point3DToggle") {
					// 	gObj.GetComponentInParent<Point3DController>().ToggleState();
					// }
				}
			} else if (Input.GetMouseButton(0) && gObj) {
				Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (objPlane.Raycast(mRay, out float rayDistance)) {
					if (xyMove) {
						float currentZ = gObj.transform.position.y;
						gObj.transform.position = mRay.GetPoint(rayDistance) + m0;
						gObj.transform.position = new Vector3(gObj.transform.position.x, currentZ, gObj.transform.position.z);
					} else if (rotationMode) {
						Vector3 r = mRay.GetPoint(rayDistance) - gObj.transform.position;
						Vector3 rt = Quaternion.AngleAxis(1, objPlane.normal) * r0;
						float a1 = Vector3.Angle(r0, r);
						float a2 = Vector3.Angle(rt, r);
						if (a2 > a1) a1 *= -1;
						gObj.transform.RotateAroundLocal(objPlane.normal, Mathf.Deg2Rad * a1);
						r0 = r;
					} else if (zMove) {
						float currentX = gObj.transform.position.x;
						float currentY = gObj.transform.position.z;
						gObj.transform.position = mRay.GetPoint(rayDistance) + m0;
						gObj.transform.position = new Vector3(currentX, gObj.transform.position.y, currentY);
					}
				}
			} else if (Input.GetMouseButtonUp(0) && gObj) {
				zMove = false;
				rotationMode = false;
				xyMove = false;
				gObj = null;
			}
		}
	}
}