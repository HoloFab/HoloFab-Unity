//#define DEBUG
#undef DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using HoloFab;

namespace HoloFab {
	// Unity Component Interfacing UDP Send class for UI.
	public class UDPSendComponent : MonoBehaviour {
		[Header("Necessary Variables.")]
		[Tooltip("A port for UDP communication to send to.")]
		public int remotePortOverride = 8055;
        [Tooltip("Received IP address of the computer.")]
        public string remoteIP = null;
        
		public void SendUI(byte[] data) {
			if (this.remoteIP != null) // just in case
				UDPSend.Send(data, this.remoteIP, this.remotePortOverride);
		}
	}
}