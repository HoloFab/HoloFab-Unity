//#define DEBUG
#undef DEBUG

using System;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

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
        // - CPlane object tag.
        private string tagCPlane = "CPlane";
        // - Local reference of CPlane object
        private GameObject cPlane;

        // Unity Functions.
        void OnEnable() {
			TCPReceive.TryStartConnection(this.localPortOverride);
		}
		void OnDisable() {
			TCPReceive.StopConnection();
		}
		void Update() {
            if (this.cPlane == null) {
			    this.cPlane = GameObject.FindGameObjectWithTag(this.tagCPlane);
			    #if DEBUG
			    Debug.Log("TCPReceive Component: CPlane: " + this.cPlane);
			    #endif
			    if (this.cPlane == null) return;
            }

			if (!TCPReceive.flagDataRead) {
				TCPReceive.flagDataRead = true;
				InterpreteData(TCPReceive.dataMessages[TCPReceive.dataMessages.Count-1]);
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
	}
}