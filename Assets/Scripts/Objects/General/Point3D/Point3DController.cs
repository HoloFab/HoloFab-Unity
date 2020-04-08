using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

namespace HoloFab {
	public class Point3DController : MonoBehaviour {
		// TODO: This should be in animation
		public Vector3 scaleClosed = new Vector3(0.01f, 0.01f, 0.01f);
		public Vector3 scaleOpen = new Vector3(10f, 1f, 10f);
        
		private bool flagOpen;
		GameObject zAxis, yAxis, xAxis, xyPlane, trigger;
		Animator xAnim, yAnim, zAnim, triggerAnim;
        
		// Label variables
		private TextMeshProUGUI textHolder;
		private Vector3 relativePosition;
        
		void Start() {
			this.flagOpen = false;
			foreach (Transform child in transform) {
				if (child.name == "z") this.zAxis = child.gameObject;
				if (child.name == "y") this.yAxis = child.gameObject;
				if (child.name == "x") this.xAxis = child.gameObject;
				if (child.tag == "CPlaneXY") this.xyPlane = child.gameObject;
				if (child.tag == "Point3DToggle") this.trigger = child.gameObject;
			}
			this.zAnim       = this.zAxis.GetComponent<Animator>();
			this.yAnim       = this.yAxis.GetComponent<Animator>();
			this.xAnim       = this.xAxis.GetComponent<Animator>();
			this.triggerAnim = this.trigger.GetComponent<Animator>();
		}
        
		// Update is called once per frame
		void Update() {
			if (!this.flagOpen)
				UpdateAnimationState();
			else
				UpdateAnimationState(false);
			//Should it be updated every frame?
			UpdatePointLabel();
		}
		// General way to update animations.
		// TODO: Make one animation for all of them together
		private void UpdateAnimationState(bool flagState=true){
			this.xAnim.SetBool("Expand", flagState);
			this.yAnim.SetBool("Expand", flagState);
			this.zAnim.SetBool("Expand", flagState);
			this.triggerAnim.SetBool("Expand", flagState);
			this.xyPlane.transform.localScale = (flagState) ? scaleClosed : scaleOpen;
		}
		// Accessible way to triger animation change.
		public void ToggleState(){
			this.flagOpen = !this.flagOpen;
		}
		////////////////////////////////////////////////////////////////////////
		// A function to Update the point Label.
		public void UpdatePointLabel(){
			if (!ObjectManager.instance.CheckCPlane()) return;
			this.relativePosition = transform.position - ObjectManager.instance.cPlane.transform.position;
			if (this.textHolder == null)
				this.textHolder = GetComponentInChildren<TextMeshProUGUI>();
			this.textHolder.text = "X: " + this.relativePosition.x.ToString() + "\n" +
			                       "Y: " + this.relativePosition.z.ToString() + "\n" +
			                       "Z: " + this.relativePosition.y.ToString();
		}
	}
}