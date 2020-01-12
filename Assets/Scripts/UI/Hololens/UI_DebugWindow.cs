///////////////// Debug Flag /////////////////
#define DEBUG
// #undef DEBUG

using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UI_DebugWindow : MonoBehaviour {
	#if DEBUG
	public TextMeshProUGUI label;
	string[] logs;
	int i = 0;
	int size = 50;
	string labelMessage;
    
	void OnEnable(){
		//this.label = GetComponent<TextMeshProUGUI>();
		Application.logMessageReceived += LogMessage;
		this.logs = new string[this.size];
	}
	void OnDisable(){
		Application.logMessageReceived -= LogMessage;//logMessageReceivedThreaded
	}
	public void LogMessage(string logString, string stackTrace, LogType type){
		this.logs[this.i] = logString;
		this.labelMessage = string.Join("\n", this.logs);
		this.label.text = labelMessage;
		this.i = (this.i + 1) % this.size;
	}
	#endif
}