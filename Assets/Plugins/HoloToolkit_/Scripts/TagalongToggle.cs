using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

[RequireComponent(typeof(Tagalong))]
public class TagalongToggle : MonoBehaviour {
	private Tagalong tagalong;
	// Start is called before the first frame update
	void Start() {
		this.tagalong = GetComponent<Tagalong>();
	}
    
	public void ToggleTagalong(){
		this.tagalong.enabled = !this.tagalong.enabled;
	}
}