#define DEBUG
//#undef DEBUG

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
		// - TCP Receiver
		private TCPReceive tcpReceiver;
		// - last interpreted message.
		private string lastMessage = "";
		// temp:
		public List<string> debugMessages = new List<string>();
		private int count=0;
        
        
		// Unity Functions.
		void OnEnable() {
			this.tcpReceiver = new TCPReceive(this.localPortOverride);
		}
		void OnDisable() {
			this.tcpReceiver.StopConnection();
		}
		void Update() {
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
			// If data found - react
			if (!this.tcpReceiver.flagDataRead) {
				UnityUtilities.UniversalDebug("Parsing meshes . . .");
				InterpreteData(this.tcpReceiver.dataMessages[this.tcpReceiver.dataMessages.Count-1]);
				this.tcpReceiver.flagDataRead = true;
			}
			// Temp - extract data
			if ((this.count == 0) || (this.count < this.tcpReceiver.debugMessages.Count)) {
				for (int i = this.count; i < this.tcpReceiver.debugMessages.Count; i++) {
					#if DEBUG
					UnityUtilities.UniversalDebug(this.tcpReceiver.debugMessages[i]);
					#endif
				}
				this.count = this.tcpReceiver.debugMessages.Count;
			}
		}
		/////////////////////////////////////////////////////////////////////////////
		// A function responsible for decoding and reacting to received TCP data.
		private void InterpreteData(string message) {
			if ((!string.IsNullOrEmpty(message)) && (this.lastMessage != message)) {
				this.lastMessage = message;
				#if DEBUG
				UnityUtilities.UniversalDebug("TCPReceive Component: New message found: " + message);
				#endif
				string[] messageComponents;
				messageComponents = message.Split(new char[] { '|' }, 2);
                
				string header = messageComponents[0];
				#if DEBUG
				UnityUtilities.UniversalDebug("TCPReceive Component: Header: " + header);
				#endif
				if (header == "MESHSTREAMING") {
					InterpreteMesh(messageComponents[1], SourceType.TCP);
				} else if (header == "HOLOBOTS") {
					InterpreteHoloBots(messageComponents[1]);
				} else {
					#if DEBUG
					UnityUtilities.UniversalDebug("TCPReceive Component: Header Not Recognized");
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