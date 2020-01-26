using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ponit3d : MonoBehaviour
{
    private string xPos, yPos, zPos;
    private GameObject cplane;
    private string tagCPlane = "CPlane";
    // Start is called before the first frame update
    void Start()
    {
        this.cplane = GameObject.FindGameObjectWithTag(this.tagCPlane);
    }

    // Update is called once per frame
    void Update()
    {
        xPos = (transform.position.x - this.cplane.transform.position.x).ToString();
        yPos = (transform.position.z - this.cplane.transform.position.z).ToString();
        zPos = (transform.position.y - this.cplane.transform.position.y).ToString();
        GetComponent<TextMesh>().text = "X: " + xPos + "\n" + "Y: " + yPos + "\n" + "Z: " + zPos;
    }
}
