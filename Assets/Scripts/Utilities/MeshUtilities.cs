using System;
using System.Collections.Generic;

using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Tools for processing meshes.
	public static partial class MeshUtilities {
        // A function to decode extracted data into Unity Mesh object.
		public static Mesh DecodeMesh(List<Vector3> currentVertices, List<int> currentFaces, List<Color> currentColors=null) {
			Mesh mesh = new Mesh();// { name = name };
			mesh.SetVertices(currentVertices);
			//mesh.SetNormals(normals);
			mesh.SetTriangles(currentFaces, 0);
			mesh.SetColors(currentColors);
			mesh.RecalculateNormals();
			return mesh;
		}
	}
}