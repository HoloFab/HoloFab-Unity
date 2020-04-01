#define DEBUG
// #define DEBUG2
//#undef DEBUG
#undef DEBUG2

using System;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;
using System.Threading;

/// <summary>
/// Receives Data from MeshStreaming and Positioner Component
/// </summary>


namespace HoloFab {
	// Unity Component Interfacing TCP Receive class.
	public class TCPReceiveComponent : MonoBehaviour {
		[Header("Necessary Variables.")]
		[Tooltip("A port for UDP communication to listen on.")]
		public int localPortOverride = 11111;
        
		// Local Variables.
		// - last interpreted message.
		private string lastMessage = "";
		// // - CPlane object tag.
		// private string tagCPlane = "CPlane";
		// // - Local reference of CPlane object
		// private GameObject cPlane;
        
		// temp:
		public List<string> debugMessages = new List<string>();
		private int count=0;
        
		TCPReceive tcp;
        
		// Unity Functions.
		void OnEnable() {
			#if UNITY_ANDROID
			UnityUtilities.UniversalDebug("Hollo World . . .");
			Thread.Sleep(1500);
			UnityUtilities.UniversalDebug("Your IP is:\n" + NetworkUtilities.LocalIPAddress());
			Thread.Sleep(3500);
			#endif
			if (this.tcp != null)
				this.tcp.StopConnection();
			this.tcp = new TCPReceive(localPortOverride);
			this.tcp.TryStartConnection(this.localPortOverride);
		}
		void OnDisable() {
			this.tcp.StopConnection();
		}
		void Update() {
			if (!this.tcp.flagConnectionFound) {
				#if DEBUG2
				Debug.Log("TCPReceive Component: Connection Found: " + this.tcp.flagConnectionFound);
				#endif
				this.tcp.TryStartConnection(this.localPortOverride);
				if (!this.tcp.flagConnectionFound) return;
			}
			// if (this.cPlane == null) {
			// 	UnityUtilities.UniversalDebug("CPlane not found. Set it by touching scanned grid.");
			// 	this.cPlane = GameObject.FindGameObjectWithTag(this.tagCPlane);
			// 	#if DEBUG
			// 	Debug.Log("TCPReceive Component: CPlane: " + this.cPlane);
			// 	#endif
			// 	if (this.cPlane == null) return;
			// }
			if (!this.tcp.flagDataRead) {
				UnityUtilities.UniversalDebug("Parsing input . . .");
				InterpreteData(this.tcp.dataMessages[this.tcp.dataMessages.Count-1]);
				this.tcp.flagDataRead = true;
			}
			if ((count == 0) || (count < tcp.debugMessages.Count)) {
				for (int i = count; i < tcp.debugMessages.Count; i++) {
					#if DEBUG
					Debug.Log(tcp.debugMessages[i]);
					#endif
				}
				count = tcp.debugMessages.Count;
			}
		}
		/////////////////////////////////////////////////////////////////////////////
		// A function responsible for decoding and reacting to received TCP data.
		private void InterpreteData(string message) {
			if ((!string.IsNullOrEmpty(message)) && (this.lastMessage != message)) {
				this.lastMessage = message;
				#if DEBUG
				Debug.Log("TCPReceive Component: New message found: " + message);
				#endif
				string[] messageComponents;
				messageComponents = message.Split(new char[] { '|' }, 2);
                
				string header = messageComponents[0];
				#if DEBUG
				Debug.Log("TCPReceive Component: Header: " + header);
				#endif
				if (header == "MESHSTREAMING") {
					InterpreteMesh(messageComponents[1], SourceType.TCP);
				} else if (header == "HOLOBOTS") {
					InterpreteHoloBots(messageComponents[1]);
				} else {
					#if DEBUG
					Debug.Log("TCPReceive Component: Header Not Recognized");
					#endif
				}
			}
		}
		// Functions to interprete and react to determined type of messages: // TODO: Join with UDP interpreters?
		// - Mesh
		private void InterpreteMesh(string data, SourceType meshSourceType){
			ObjectManager.instance.GetComponent<MeshProcessor>().ProcessMesh(EncodeUtilities.InterpreteMesh(data), meshSourceType);
		}
		// - HoloBots
		private void InterpreteHoloBots(string data){
			ObjectManager.instance.GetComponent<RobotProcessor>().ProcessRobot(EncodeUtilities.InterpreteHoloBots(data));
		}
        
		public void _ShowAndroidToastMessage(string message)
		{
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject unityActivity =
				unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
			if (unityActivity != null) {
				AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
				unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
				{
					AndroidJavaObject toastObject =
						toastClass.CallStatic<AndroidJavaObject>(
						                                         "makeText", unityActivity, message, 0);
					toastObject.Call("show");
				}));
			}
		}
	}
}