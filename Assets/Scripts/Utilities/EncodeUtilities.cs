using System;
using System.Collections.Generic;

using System.Text;
using Newtonsoft.Json;

using HoloFab.CustomData;

namespace HoloFab {
	// Tools for processing robit data.
	public static partial class EncodeUtilities {
		// Encode data into a json readable byte array.
		public static byte[] EncodeData(string header, System.Object item, out string message){
			string output = JsonConvert.SerializeObject(item);
			if (header != string.Empty)
				message = header + "|" + output;
			else
				message = output;
			return Encoding.UTF8.GetBytes(message);
        }
        // Decode Data into a string.
        public static string DecodeData(byte[] data) {
            return Encoding.UTF8.GetString(data);
        }
        // Decode Data into a string.
        public static string DecodeData(byte[] data, int index, int count) { 
            return Encoding.UTF8.GetString(data, index, count);
        }
    }
	/////////////////////////////////////////////////////////////////////////////
	// Unity only side.
	public static partial class EncodeUtilities {
		public static List<MeshData> InterpreteMesh(string data){
			return JsonConvert.DeserializeObject<List<MeshData> >(data);
		}
		public static IList<RobotData> InterpreteHoloBots(string data){
			return JsonConvert.DeserializeObject<IList<RobotData> >(data);
		}
		public static IList<RobotControllerData> InterpreteRobotController(string data){
			return JsonConvert.DeserializeObject<IList<RobotControllerData> >(data);
		}
		public static TagData InterpreteTag(string data){
			return JsonConvert.DeserializeObject<TagData>(data);
		}
		public static string InterpreteIPAddress(string data){
			return data.Replace("\"", string.Empty);
		}
	}
}