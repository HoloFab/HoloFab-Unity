//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Generatable UI Controlling manager.
	// TODO:
	// - Move canvases height offset (55) up.
	// - Check if generic version works. If so delete commented code.
	// - Later: Make into more generic list of types with corresponding variables.
	// - Later: Automatically find buttons and canvasses by tags (?)
	public class UIManager : MonoBehaviour {
		// Static accessor.
		private static UIManager _instance;
		public static UIManager instance {
			get {
				if (UIManager._instance == null)
					UIManager._instance = FindObjectOfType<UIManager>();
				return UIManager._instance;
			}
		}

        // Necessary variables.
        [Header("Variables Set From Scene.")]
        [Tooltip("Buttons to control each type of generatable UI.")]
        public Button buttonBooleanAdder;
        [Tooltip("Buttons to control each type of generatable UI.")]
        public Button buttonCounterAdder;
        [Tooltip("Buttons to control each type of generatable UI.")]
        public Button buttonSliderAdder;
        [Tooltip("Button to delete UI items.")]
        public Button buttonDeleter;
        [Tooltip("Parent Canvases for each type of generatable UI.")]
        public Canvas canvasBooleanToggle, canvasCounter, canvasSlider;

        [Header("Variables Set From Prefabs.")]
        [Tooltip("Prefab Prefabs of each type of generatable UI.")]
        public GameObject goPrefabUIBooleanToggle;
        [Tooltip("Prefab Prefabs of each type of generatable UI.")]
        public GameObject goPrefabUICounter;
        [Tooltip("Prefab Prefabs of each type of generatable UI.")]
        public GameObject goPrefabUISlider;

        // Secondary Variables with presets.
        [Header("Preset Variables.")]
        [Tooltip("Tags for generatable UI items.")]
        public string tagUIItemBoolean = "Toggle";
        [Tooltip("Tags for generatable UI items.")]
        public string tagUIItemCounter = "Counter";
        [Tooltip("Tags for generatable UI items.")]
        public string tagUIItemSlider = "Slider";
		[Tooltip("Limiting amounts for each type of generatable UI.")]
		public int limitBooleanToggle = 6, limitCounter = 6, limitSlider = 7;
        
		// Local variables.
		// - UI item amounts to keep track.
		private int amountBooleanToggle = 0, amountCounter = 0, amountSlider = 0;
		// - Heights of canvases to be calculated on Start.
		private float heightCanvasBooleanToggle, heightCanvasCounter, heightCanvasSlider;
        
		// Network variables.
		// Stored message to avoid unnecessary traffic.
		private static string lastMessage;
		// Reference to the Sender.
		private static UDPSendComponent sender;
        
		void Start() {
			// Set up the buttons. // Should be in code?
			if (this.buttonBooleanAdder != null)
				this.buttonBooleanAdder.onClick.AddListener(TryAddBooleanToggle);
			if (this.buttonCounterAdder != null)
				this.buttonCounterAdder.onClick.AddListener(TryAddCounter);
			if (this.buttonSliderAdder != null)
				this.buttonSliderAdder.onClick.AddListener(TryAddSlider);
			if (this.buttonDeleter != null)
				this.buttonDeleter.onClick.AddListener(DeleteGeneratedUI);
			// Extract Canvas Heights.
			if (this.canvasBooleanToggle != null)
				this.heightCanvasBooleanToggle = this.canvasBooleanToggle.gameObject.GetComponent<RectTransform>().rect.height;
			if (this.canvasCounter != null)
				this.heightCanvasCounter = this.canvasCounter.gameObject.GetComponent<RectTransform>().rect.height;
			if (this.canvasSlider != null)
				this.heightCanvasSlider = this.canvasSlider.gameObject.GetComponent<RectTransform>().rect.height;
		}
		//////////////////////////////////////////////////////////////////////////
		// Generic UI adding function.
		private void TryAddUIItem(ref int amount, int limit, GameObject goPrefab, Canvas cParent, float height) {
			if (amount < limit) {
				#if DEBUG
				Debug.Log("UIManager: Adding new UI Element.");
				#endif
                
				GameObject goUIItem = Instantiate(goPrefab, cParent.gameObject.transform);
				RectTransform rectTransform = goUIItem.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(0, amount * height / limit);
				amount++;
                
                // Inform UI Manager.
                UIManager.instance.OnValueChanged();
            }
		}
		// Add Boolean Toggle UI item.
		private void TryAddBooleanToggle() {
			TryAddUIItem(ref this.amountBooleanToggle, this.limitBooleanToggle,
			             this.goPrefabUIBooleanToggle, this.canvasBooleanToggle,
			             this.heightCanvasBooleanToggle);
		}
		// Add Counter UI item.
		private void TryAddCounter() {
			TryAddUIItem(ref this.amountCounter, this.limitCounter,
			             this.goPrefabUICounter, this.canvasCounter,
			             this.heightCanvasCounter);
		}
		// Add Slider UI item.
		private void TryAddSlider() {
			TryAddUIItem(ref this.amountSlider, this.limitSlider,
			             this.goPrefabUISlider, this.canvasSlider,
			             this.heightCanvasSlider);
		}
		// Delete all user generated UIs.
		private void DeleteGeneratedUI() {
			GameObject[] goBooleans = GameObject.FindGameObjectsWithTag(this.tagUIItemBoolean);
			GameObject[] goCounters = GameObject.FindGameObjectsWithTag(this.tagUIItemCounter);
			GameObject[] goSliders = GameObject.FindGameObjectsWithTag(this.tagUIItemSlider);
            
			for (int i = goBooleans.Length-1; i >= 0; i--) DestroyImmediate(goBooleans[i]);
			for (int i = goCounters.Length-1; i >= 0; i--) DestroyImmediate(goCounters[i]);
			for (int i = goSliders.Length-1; i >= 0; i--) DestroyImmediate(goSliders[i]);
			Resources.UnloadUnusedAssets();
            
			this.amountBooleanToggle = 0;
			this.amountCounter = 0;
			this.amountSlider = 0;

            // Inform UI Manager.
            UIManager.instance.OnValueChanged();
        }
        
		//////////////////////////////////////////////////////////////////////////
		// React to a value change.
		public void OnValueChanged() {
			if (UDPReceiveComponent.flagUICommunicationStarted) {
				#if DEBUG
				Debug.Log("UIManager: Updating UI values.");
				#endif
				// Find Objects.
				GameObject[] goBooleans = GameObject.FindGameObjectsWithTag(this.tagUIItemBoolean);
				GameObject[] goCounters = GameObject.FindGameObjectsWithTag(this.tagUIItemCounter);
				GameObject[] goSliders = GameObject.FindGameObjectsWithTag(this.tagUIItemSlider);
				#if DEBUG
                #endif
				Debug.Log("UIManager: Found items: booleans: " + goBooleans.Length + ", counters: " + goCounters.Length + ", sliders: " + goSliders.Length);

				// Extract data.
				IList<bool> bools = new List<bool>();
				IList<int> ints = new List<int>();
				IList<float> floats = new List<float>();
				foreach (GameObject goItem in goBooleans)
					bools.Add(goItem.GetComponent<BooleanToggle>().value);
				foreach (GameObject goItem in goCounters)
					ints.Add(goItem.GetComponent<Counter>().value);
				foreach (GameObject goItem in goSliders)
					floats.Add(goItem.GetComponent<FloatSlider>().value);
				UIData ui = new UIData(bools, ints, floats);
                
				// Encode and if changed - send it.
				byte[] data = EncodeUtilities.EncodeData(string.Empty, ui, out string message);
				if (UIManager.lastMessage != message) { // TODO: Technically not necessary now since we call directly from UI elements themselves.
					UIManager.lastMessage = message;
					#if DEBUG
					Debug.Log("UIManager: values changed, sending: " + message);
					#endif
                    
					if (UIManager.sender == null) UIManager.sender = FindObjectOfType<UDPSendComponent>();
					if (UIManager.sender == null) return;
					UIManager.sender.SendUI(data);
				}
			}
		}
	}
}