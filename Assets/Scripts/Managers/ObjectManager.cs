//#define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using System.Threading;
using GoogleARCore.Examples.HelloAR;
#endif

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Generatable Object manager.
	// TODO:
	// - Later: Move processors here?
	[RequireComponent(typeof(MeshProcessor))]
	[RequireComponent(typeof(RobotProcessor))]
	[RequireComponent(typeof(TagProcessor))]
	[RequireComponent(typeof(Point3DProcessor))]
	public class ObjectManager : Type_Manager<ObjectManager> {
		// - CPlane object tag.
		private string tagCPlane = "CPlane";
		// - Local reference of CPlane object
		public GameObject cPlane;
        
		// Local Variables.
		private string sourceName = "Object Manager";
        
		void OnEnable(){
			DebugUtilities.UserMessage("Hollo World . . .");
			#if UNITY_ANDROID
			Thread.Sleep(1500);
			#endif
			DebugUtilities.UserMessage("Your IP is:\n" + NetworkUtilities.LocalIPAddress());
			#if UNITY_ANDROID
			Thread.Sleep(3500);
			#endif
		}
		// If c plane is not found - hint user and return false.
		public bool CheckCPlane(){
			if (this.cPlane == null) {
				this.cPlane = GameObject.FindGameObjectWithTag(this.tagCPlane);
				if (this.cPlane == null) {
					DebugUtilities.UserMessage("Place your CPlane by tapping on scanned mesh.");
					return false;
				}
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "CPlane: " + this.cPlane);
				#endif
			}
            HoloFabARController.cPlaneInstance = this.cPlane;
            return true;
		}
        //public void Update(){
        //    if (!CheckCPlane()) return;
        //    this.transform.position = this.cPlane.transform.position;
        //}
    }
}