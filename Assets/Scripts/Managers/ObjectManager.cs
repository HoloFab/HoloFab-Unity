//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Generatable Object manager.
	// TODO:
	// - Later: Move processors here?
	public class ObjectManager : MonoBehaviour {
		// Static accessor.
		private static ObjectManager _instance;
		public static ObjectManager instance {
			get {
				if (ObjectManager._instance == null)
					ObjectManager._instance = FindObjectOfType<ObjectManager>();
				return ObjectManager._instance;
			}
		}
        
		// - CPlane object tag.
		private string tagCPlane = "CPlane";
		// - Local reference of CPlane object
		public static GameObject cPlane;
        
		void OnEnable(){
			UnityUtilities.UniversalDebug("Hollo World . . .");
			Thread.Sleep(1500);
			UnityUtilities.UniversalDebug("Your IP is:\n" + NetworkUtilities.LocalIPAddress());
			Thread.Sleep(3500);
		}
		// If c plane is not found - hint user and return false.
		public bool CheckCPlane(){
			if (ObjectManager.cPlane == null) {
				UnityUtilities.UniversalDebug("Place your CPlane by tapping on scanned mesh.");
				ObjectManager.cPlane = GameObject.FindGameObjectWithTag(this.tagCPlane);
				#if DEBUG
				UnityUtilities.UniversalDebug("UDPReceive Component: CPlane: " + ObjectManager.cPlane);
				#endif
				if (ObjectManager.cPlane == null) return false;
			}
			return true;
		}
	}
}