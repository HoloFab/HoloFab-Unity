//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
    // Generatable Object manager.
    // TODO:
    // - Later: Move processors here?
    public class ObjectManager : MonoBehaviour {
        // Static accessor.
        private static ObjectManager _instance;
        public static ObjectManager instance {
            get {
                if (ObjectManager._instance == null)
                    ObjectManager._instance = FindObjectOfType<ObjectManager>();
                return ObjectManager._instance;
            }
        }
    }
}