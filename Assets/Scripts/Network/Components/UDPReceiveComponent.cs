//#define DEBUG
#undef DEBUG

using System;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Unity Component Interfacing UDP Receive class.
	public class UDPReceiveComponent : MonoBehaviour {
		[Header("Necessary Variables.")]
		[Tooltip("A port for UDP communication to listen on.")]
		public int localPortOverride = 12121;
        
		// Local Variables.
		// - UDP sender
		private UDPReceive udpReceiver;
		// - IP Address received.
		[HideInInspector]
		public static bool flagUICommunicationStarted = false;
		// - last interpreted message.
		private string lastMessage = "";
        
		// Unity Functions.
		void OnEnable() {
			this.udpReceiver = new UDPReceive(this.localPortOverride);
		}
		void OnDisable() {
			this.udpReceiver.StopConnection();
		}
		void Update() {
			// Check for C-plane
			if (!ObjectManager.instance.CheckCPlane()) return;
			// If data found - react
			if (!this.udpReceiver.flagDataRead) {
				this.udpReceiver.flagDataRead = true;
				InterpreteData(this.udpReceiver.dataMessages[this.udpReceiver.dataMessages.Count-1]);
			}
		}
		/////////////////////////////////////////////////////////////////////////////
		// A function responsible for decoding and reacting to received UDP data.
		private void InterpreteData(string message) {
			if ((!string.IsNullOrEmpty(message)) && (this.lastMessage != message)) {
				this.lastMessage = message;
				#if DEBUG
				UnityUtilities.UniversalDebug("UDPReceive Component: New message found: " + message);
				#endif
				string[] messageComponents;
				messageComponents = message.Split('|');
                
				string header = messageComponents[0];
				#if DEBUG
				UnityUtilities.UniversalDebug("UDPReceive Component: Header: " + header);
				#endif
				if (header == "MESHSTREAMINGPLUS") {
					InterpreteMesh(messageComponents[1], SourceType.UDP);
				} else if (header == "CONTROLLER") {
					InterpreteRobotController(messageComponents[1]);
				} else if (header == "HOLOTAG") {
					InterpreteTag(messageComponents[1]);
				} else if(header == "IPADDRESS") {
					InterpreteIPAddress(messageComponents[1]);
				} else {
					#if DEBUG
					UnityUtilities.UniversalDebug("UDPReceive Component: Header Not Recognized");
					#endif
				}
			}
		}
		// Functions to interprete and react to determined type of messages:
		// - Mesh
		private void InterpreteMesh(string data, SourceType meshSourceType){
			ObjectManager.instance.GetComponent<MeshProcessor>().ProcessMesh(EncodeUtilities.InterpreteMesh(data), meshSourceType);
		}
		// - RobotControllers
		private void InterpreteRobotController(string data){
			List<RobotControllerData> controllersData = EncodeUtilities.InterpreteRobotController(data);
            
			RobotProcessor processor = ObjectManager.instance.GetComponent<RobotProcessor>();
			foreach (RobotControllerData controllerData in controllersData)
				if(processor.robots.ContainsKey(controllerData.robotID))
					processor.robots[controllerData.robotID].GetComponentInChildren<RobotController>().ProcessRobotController(controllerData);
			// GameObject[] gos = GameObject.FindGameObjectsWithTag("Axis");
			// for (int i = 0; i < gos.Length; i++) {
			// 	gos[i].GetComponent<RobotController>().ProcessRobotController(EncodeUtilities.InterpreteRobotController(data));
			// }
		}
		// - Tag
		private void InterpreteTag(string data){
			ObjectManager.instance.GetComponent<TagProcessor>().ProcessTag(EncodeUtilities.InterpreteTag(data));
		}
		// - IP address
		private void InterpreteIPAddress(string data){
			UDPSendComponent sender = gameObject.GetComponent<UDPSendComponent>();
			sender.remoteIP = EncodeUtilities.InterpreteIPAddress(data);
			UDPReceiveComponent.flagUICommunicationStarted = true;
			// Inform UI Manager.
			ParameterUIMenu.instance.OnValueChanged();
		}
	}
}