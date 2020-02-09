using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// A class Holding generally useful functions for Unity Interaction.
	public static class UnityUtilities {
		// Calculate Ray from near plane of frustum to far plane based on mouse position.
		public static Ray GenerateMouseRay() {
			Vector3 mousePosFar = new Vector3(Input.mousePosition.x,
			                                  Input.mousePosition.y,
			                                  Camera.main.farClipPlane);
			Vector3 mousePosNear = new Vector3(Input.mousePosition.x,
			                                   Input.mousePosition.y,
			                                   Camera.main.nearClipPlane);
            
			Vector3 mousePosF = Camera.main.ScreenToWorldPoint(mousePosFar);
			Vector3 mousePosN = Camera.main.ScreenToWorldPoint(mousePosNear);
            
			Ray ray = new Ray(mousePosN, mousePosF);
			return ray;
		}
		// Extract angle of the vector projected on a plane perpendicular to a given normal and around given normal axis.
		// Return in degrees.
		public static float AngleFromDirection(Vector3 direction, Vector3 planeNormal) {
			// Project vector on the plane.
			Vector3 projectedDirection = Vector3.ProjectOnPlane(direction, planeNormal);
			Quaternion correction = Quaternion.FromToRotation(planeNormal, Vector3.up);
			projectedDirection = correction * projectedDirection;
			// Calculate the angle.
			Vector2 temp = new Vector2(direction.x, direction.z).normalized;
			return Mathf.Atan2(temp.x, temp.y) * Mathf.Rad2Deg;
		}
        
		public static void UniversalDebug(string message){
			#if UNITY_ANDROID
			AndroidUtilities.ToastMessage(message);
			// #elif WINDOWS_UWP
			#else
			Debug.Log(message);
			#endif
		}
	}
    
	public static class AndroidUtilities {
		public static void ToastMessage(string message) {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); // Shouldn't this be Holofab?
			AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
			if (unityActivity != null) {
				AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
				unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
					AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText",
					                                                                         unityActivity,
					                                                                         message, 0);
					toastObject.Call("show");
				}));
			}
		}
	}
}