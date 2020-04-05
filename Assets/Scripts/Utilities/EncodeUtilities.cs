using System;
using System.Collections.Generic;

using System.Text;
using Newtonsoft.Json;

using HoloFab.CustomData;

namespace HoloFab {
	// Tools for processing robit data.
	public static partial class EncodeUtilities {
		public static string headerSplitter = "|";
		public static string messageSplitter = "~";
		// Encode data into a json readable byte array.
		public static byte[] EncodeData(string header, System.Object item, out string message){
			string output = JsonConvert.SerializeObject(item);
			if (header != string.Empty)
				message = header + EncodeUtilities.headerSplitter + output;
			else
				message = output;
			message += EncodeUtilities.messageSplitter; // End Message Char
			return Encoding.UTF8.GetBytes(message);
		}
		// If message wsn't stripped - remove the message splitter
		public static string StripSplitter(string message){
			if (message.EndsWith(EncodeUtilities.messageSplitter))
				return message.Substring(0, message.Length - 1);
			return message;
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
		public static List<RobotData> InterpreteHoloBots(string data){
			return JsonConvert.DeserializeObject<List<RobotData> >(data);
		}
		public static List<RobotControllerData> InterpreteRobotController(string data){
			return JsonConvert.DeserializeObject<List<RobotControllerData> >(data);
		}
		public static TagData InterpreteTag(string data){
			return JsonConvert.DeserializeObject<TagData>(data);
		}
		public static string InterpreteIPAddress(string data){
			return data.Replace("\"", string.Empty);
		}
	}
}