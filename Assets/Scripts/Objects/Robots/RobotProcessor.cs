//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Process Received Robots and update relevant gameObjects.
	public class RobotProcessor : MonoBehaviour {
		[Tooltip("Prefab of a robots.")]
		public GameObject goPrefabKukaKR150;
		[Tooltip("Prefab of a robots.")]
		public GameObject goPrefabABB120;
		[Tooltip("Prefab of a robots.")]
		public GameObject goPrefabABB140;
		// - Local reference of CPlane object
		private GameObject cPlane;
		// - Generated Object Tags.
		private string tagKukaKR150 = "Kuka_KR150";
		private string tagABB120 = "ABB_120";
		private string tagABB140 = "ABB_140";
		// - Tool tag
		private string tagTool = "tool_Object";
		// - CPlane object tag.
		private string tagCPlane = "CPlane";
		// - Keep track of robots by ids.
		[HideInInspector]
		public Dictionary<int, GameObject> robots = new Dictionary<int, GameObject>();
        
		// Decode Received Data.
		public void ProcessRobot(List<RobotData> receivedRobots) {
			#if DEBUG
			Debug.Log("Robot: got robots: " + receivedRobots.Count);
			#endif
			cPlane = GameObject.FindGameObjectWithTag(this.tagCPlane);
			#if DEBUG
			Debug.Log("Robot: CPlane: " + cPlane);
			#endif
			if (cPlane == null) return;
            
			// Loop through all received robots.
			for (int i = 0; i < receivedRobots.Count; i++) {
				int robotID = receivedRobots[i].robotID;
				string bot = receivedRobots[i].robotName;
				double[] basePlane = receivedRobots[i].robotPlane;
				EndeffectorData endEffector = receivedRobots[i].endEffector;
                
				#if DEBUG
				Debug.Log("Robot: " + bot);
				#endif
				if (bot == "KR150-2_110") {
					ProcessHoloBot(this.tagKukaKR150, this.goPrefabKukaKR150, basePlane, endEffector, robotID);
                    
				} else if (bot == "IRB120") {
					ProcessHoloBot(this.tagABB120, this.goPrefabABB120, basePlane, endEffector, robotID);
                    
				} else if (bot == "IRB140") {
					ProcessHoloBot(this.tagABB140, this.goPrefabABB140, basePlane, endEffector, robotID);
                    
				} else {
					#if DEBUG
					Debug.Log("Robot: robot not recognized");
					#endif
				}
			}
		}
		private void ProcessHoloBot(string tag, GameObject goPrefab, double[] basePlane, EndeffectorData endEffector, int robotID) {
			GameObject goHoloBot;
			//GameObject goHoloBot = GameObject.FindGameObjectWithTag(tag);
			// If HoloBot not found - add it.
			//if (goHoloBot == null) {
			//	#if DEBUG
			//	Debug.Log("Robot: robot doesn't exist. Creating.");
			//	#endif
			//	goHoloBot = CreateBot(this.cPlane, goPrefab, endEffector, robotID);
			//}
			if (!robots.ContainsKey(robotID)) {
				goHoloBot = CreateBot(this.cPlane, goPrefab, endEffector, robotID);
				robots.Add(robotID, goHoloBot);
			} else {
				goHoloBot = robots[robotID];
				if (goHoloBot.tag != tag) {
					DestroyImmediate(goHoloBot);
					goHoloBot = CreateBot(this.cPlane, goPrefab, endEffector, robotID);
					robots[robotID] = goHoloBot;
				}
			}
			// Update HoloBot transform.
			goHoloBot.transform.SetPositionAndRotation(this.cPlane.transform.position + new Vector3((float)basePlane[0],
			                                                                                        (float)basePlane[1],
			                                                                                        (float)basePlane[2]),
			                                           this.cPlane.transform.rotation * new Quaternion(-(float)basePlane[5],
			                                                                                           (float)basePlane[6],
			                                                                                           (float)basePlane[4],
			                                                                                           (float)basePlane[3]));
		}
        
		private GameObject CreateBot(GameObject cPlane, GameObject goPrefab, EndeffectorData endEffector, int robotID) { // int port, double[] tcp
			#if DEBUG
			Debug.Log("Robot: Instantiating");
			#endif
			GameObject goHoloBot = Instantiate(goPrefab, this.cPlane.transform.position, this.cPlane.transform.rotation, this.cPlane.transform);
			goHoloBot.GetComponentInChildren<RobotController>().robotID = robotID;
            
			foreach (MeshFilter meshFilter in goHoloBot.GetComponentsInChildren<MeshFilter>()) {
				if (meshFilter.gameObject.tag == this.tagTool) {
					#if DEBUG
					Debug.Log("Robot: Found Tool");
					#endif
					CreateMesh(endEffector, meshFilter);
				}
			}
			return goHoloBot;
		}
        
		private void CreateMesh(EndeffectorData endEffector, MeshFilter tool) { //, double[] tcp
			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
            
			if (endEffector.vertices != null) {
				#if DEBUG
				Debug.Log("Robot: Adding Verticies . . . !");
				#endif
				for (int j = 0; j < endEffector.vertices.Count; j++) {
					vertices.Add(new Vector3(endEffector.vertices[j][0],
					                         endEffector.vertices[j][1],
					                         endEffector.vertices[j][2]));
				}
			}
            
			if (endEffector.faces != null) {
				#if DEBUG
				Debug.Log("Robot: Adding Faces . . . !");
				#endif
				for (int j = 0; j < endEffector.faces.Count; j++) {
					triangles.Add(endEffector.faces[j][1]);
					triangles.Add(endEffector.faces[j][2]);
					triangles.Add(endEffector.faces[j][3]);
                    
					if ((endEffector.faces[j][0] == 1)) {
						triangles.Add(endEffector.faces[j][1]);
						triangles.Add(endEffector.faces[j][3]);
						triangles.Add(endEffector.faces[j][4]);
					}
				}
			}
			tool.mesh = MeshUtilities.DecodeMesh(vertices, triangles);
			tool.transform.Rotate(0, 180, -90);
		}
        
		public void DeleteRobots() {
			List<GameObject> robots = this.robots.Values.ToList();
			for (int i = robots.Count - 1; i >=0; i--) {
				DestroyImmediate(robots[i]);
			}
			Resources.UnloadUnusedAssets();
		}
	}
}