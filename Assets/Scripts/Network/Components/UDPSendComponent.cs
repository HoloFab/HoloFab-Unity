//#define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Unity Component Interfacing UDP Send class for UI.
	public class UDPSendComponent : MonoBehaviour {
		[Header("Necessary Variables.")]
		public UDPSend udpSender;
		[Tooltip("A port for UDP communication to send to.")]
		public int remotePortOverride = 8055;
		[Tooltip("Received IP address of the computer.")]
		public string remoteIP = null;
        
		// Local Variables.
		private string sourceName = "UDP Sender Component";
        
		public void SendUI(UIData ui) {
			Send("UIDATA", ui);
		}
        
		public void SendPoints(PointCloudData pointCloud) {
			Send("POINTCLOUD", pointCloud);
		}
        
		public void SendMesh(MeshData mesh) {
			Send("MESHSTREAMING", mesh);
		}
        
		private void Send(string header, System.Object item) {
			if (this.remoteIP != null) {// just in case
				byte[] data = EncodeUtilities.EncodeData(header, item, out string currentMessage);
				#if DEBUG
				Debug.Log("UDP Sender: sending data: " + currentMessage);
				#endif
                
				if (this.udpSender == null)
					this.udpSender = new UDPSend(this.remoteIP, this.remotePortOverride);
				this.udpSender.QueueUpData(data);
				// if (!this.udpSender.flagSuccess) {
				// 	#if DEBUGWARNING
				// 	DebugUtilities.UniversalWarning(this.sourceName, "Couldn't send data.");
				// 	#endif
				// }
			} else {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "No server IP Found - enable Grasshopper UI Receiving Component");
				#endif
			}
		}
	}
}