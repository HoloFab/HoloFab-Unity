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
		// - IP Address received.
		[HideInInspector]
		public static bool flagUICommunicationStarted = false;
		// - last interpreted message.
		private string lastMessage = "";
		// - CPlane object tag.
		private string tagCPlane = "CPlane";
		// - Local reference of CPlane object
		private GameObject cPlane;
        
		// Unity Functions.
		void OnEnable() {
			UDPReceive.TryStartConnection(this.localPortOverride);
		}
		void OnDisable() {
			UDPReceive.StopConnection();
		}
		void Update() {
			if (this.cPlane == null) {
				this.cPlane = GameObject.FindGameObjectWithTag(this.tagCPlane);
				#if DEBUG
				Debug.Log("UDPReceive Component: CPlane: " + this.cPlane);
				#endif
				if (this.cPlane == null) return;
			}
            
			if (!UDPReceive.flagDataRead) {
				UDPReceive.flagDataRead = true;
				InterpreteData(UDPReceive.dataMessages[UDPReceive.dataMessages.Count-1]);
			}
		}
		/////////////////////////////////////////////////////////////////////////////
		// A function responsible for decoding and reacting to received UDP data.
		private void InterpreteData(string message) {
			if ((!string.IsNullOrEmpty(message)) && (this.lastMessage != message)) {
				this.lastMessage = message;
				#if DEBUG
				Debug.Log("UDPReceive Component: New message found: " + message);
				#endif
				string[] messageComponents;
				messageComponents = message.Split('|');
                
				string header = messageComponents[0];
				#if DEBUG
				Debug.Log("UDPReceive Component: Header: " + header);
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
					Debug.Log("UDPReceive Component: Header Not Recognized");
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