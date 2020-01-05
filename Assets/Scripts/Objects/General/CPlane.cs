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
		// Local Variables
		// - Transformation Modes
		private bool modeRotationXY, modeMoveZ, modeMoveXY;
		// - Plane of rotation
		private Plane cPlanePlane;
		// - Last touch start location
		private Vector3 firstTouchLocation;
		// - Direction to the last touch point
		private Vector3 toLastDirection;
		// - projected angle relative to normal
		private float toLastTouchAngle;
        
		// Unity Functions.
		void Update() {
			// Check start action
			if (Input.GetMouseButtonDown(0))
				TryStartTransformation();
			// Update in action
			if (Input.GetMouseButton(0) && (this.modeRotationXY || this.modeMoveXY || this.modeMoveZ))
				UpdateTransformation();
			// Stop action
			if (Input.GetMouseButtonUp(0))
				SetModes();
		}
		/////////////////////////////////////////////////////////////////////////////
		private void TryStartTransformation() {
			Ray mouseRay = UnityUtilities.GenerateMouseRay(); // Why two rays?
			if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out RaycastHit hit)) {
				#if DEBUG
				Debug.Log("CPlane Controller: Starting the event.");
				#endif
                
				// Extract Hit Info.
				GameObject goHitItem = hit.transform.gameObject;
				this.firstTouchLocation = hit.transform.position - hit.point;
				#if DEBUG
				Debug.Log("CPlane: Touch object tag: " + goHitItem.tag);
				#endif
                
				// Determine type of transformation.
				this.cPlanePlane = new Plane(transform.up, transform.position);
				if (goHitItem.tag == "CPlaneXYR") SetModes(_modeRotationXY: true);
				else if (goHitItem.tag == "CPlaneXY") SetModes(_modeMoveXY: true);
				else if (goHitItem.tag == "CPlaneZ") SetModes(_modeMoveZ: true);
				else SetModes();
                
				Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (this.cPlanePlane.Raycast(mRay, out float rayDistance)) {
					this.toLastDirection = mRay.GetPoint(rayDistance) - transform.position;
					this.toLastTouchAngle = UnityUtilities.AngleFromDirection(this.toLastDirection,
					                                                          this.cPlanePlane.normal); // Shouldn't this be Camera.main.transform.forward
					#if DEBUG
					Debug.Log("CPlane: Original Screen Orientation: " + this.toLastTouchAngle);
					#endif
				}
			}
		}
		// Update Transformation
		private void UpdateTransformation() {
			// Send ray to screen and evaluate current rotation.
			Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (this.cPlanePlane.Raycast(mRay, out float rayDistance)) {
				Vector3 projectedPlaneRayLocation = mRay.GetPoint(rayDistance);
				if (this.modeRotationXY && !this.modeMoveXY && !this.modeMoveZ) { // Rotate XY
					float toCurrentTouchAngle = UnityUtilities.AngleFromDirection(projectedPlaneRayLocation - transform.position,
					                                                              this.cPlanePlane.normal); // Shouldn't this be Camera.main.transform.forward
					#if DEBUG
					Debug.Log("CPlane: Current orientation: " + toCurrentTouchAngle);
					#endif
					transform.RotateAround(transform.position, this.cPlanePlane.normal, -this.toLastTouchAngle + toCurrentTouchAngle);
                    this.toLastTouchAngle = toCurrentTouchAngle;
                } else if (!this.modeRotationXY && this.modeMoveXY && !this.modeMoveZ) { // Move XY
					transform.position = this.firstTouchLocation + projectedPlaneRayLocation;
				} else if (!this.modeRotationXY && !this.modeMoveXY && this.modeMoveZ) { // Move Z
					transform.localPosition += Vector3.up * (Input.GetAxis("Mouse Y") * 0.05f);
                }
            }
		}
		// Reset Modes.
		private void SetModes(bool _modeRotationXY=false, bool _modeMoveXY=false, bool _modeMoveZ=false){
			this.modeRotationXY = _modeRotationXY;
			this.modeMoveXY = _modeMoveXY;
			this.modeMoveZ = _modeMoveZ;
			#if DEBUG
			Debug.Log("CPlane: modes: XY rotation: " + this.modeRotationXY + ", move XY " + this.modeMoveXY + ", move Z " + this.modeMoveZ);
			#endif
		}
	}
}