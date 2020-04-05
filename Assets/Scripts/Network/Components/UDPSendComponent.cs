//#define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using HoloFab;

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

		public void SendUI(byte[] data) {
			if (this.remoteIP != null) {// just in case
				// if (this.udpSender == null)
				this.udpSender = new UDPSend(this.remoteIP, this.remotePortOverride);
				this.udpSender.Send(data);
			} else {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "No server IP Found - enable Grasshopper UI Receiving Component");
				#endif
			}
		}
	}
}
