using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

[RequireComponent(typeof(RadialView))]
public class ToggleRadialView : MonoBehaviour {
	private RadialView radialView;
	// Start is called before the first frame update
	void Start() {
		this.radialView = GetComponent<RadialView>();
	}
    
	public void OnToggleRadialView(){
		this.radialView.enabled = !this.radialView.enabled;
	}
}