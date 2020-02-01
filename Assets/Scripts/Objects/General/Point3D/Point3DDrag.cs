using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point3DDrag : MonoBehaviour
{
    GameObject gObj = null;
    Transform target;
    GameObject zAxis, yAxis, xAxis, xyPlane, sphere;

    Animator xAnim, yAnim, zAnim, cAnim;
    public bool open;
    float axisLength;

    Ray GenerateMouseRay()
    {
        Vector3 mousePosFar = new Vector3(Input.mousePosition.x,
                                          Input.mousePosition.y,
                                          Camera.main.farClipPlane);
        Vector3 mousePosNear = new Vector3(Input.mousePosition.x,
                                           Input.mousePosition.y,
                                           Camera.main.nearClipPlane);

        Vector3 mousePosF = Camera.main.ScreenToWorldPoint(mousePosFar);
        Vector3 mousePosN = Camera.main.ScreenToWorldPoint(mousePosNear);

        Ray mr = new Ray(mousePosN, mousePosF);
        return mr;
    }

    private void Start()
    {
        this.open = false;
        foreach (Transform child in transform)
        {
            if (child.name == "z") this.zAxis = child.gameObject;
            if (child.name == "y") this.yAxis = child.gameObject;
            if (child.name == "x") this.xAxis = child.gameObject;
            if (child.tag == "Point3DToggle") this.sphere = child.gameObject;
            if (child.tag == "CPlaneXY") this.xyPlane = child.gameObject;
        }
        this.xAnim = this.xAxis.GetComponent<Animator>();
        this.yAnim = this.yAxis.GetComponent<Animator>();
        this.zAnim = this.zAxis.GetComponent<Animator>();
        this.cAnim = this.sphere.GetComponent<Animator>();

        this.xAnim.SetBool("Expand", true);
        this.yAnim.SetBool("Expand", true);
        this.zAnim.SetBool("Expand", true);
        this.cAnim.SetBool("Expand", true);
        this.xyPlane.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Ray mouseRay = GenerateMouseRay();
        //    RaycastHit hit;

        //    if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit))
        //    {
        //        gObj = hit.transform.gameObject;
        //        if (gObj.tag == "Point3DToggle")
        //        {
        //            this.open = !this.open;
                    if (!this.open)
                    {
                        this.xAnim.SetBool("Expand", true);
                        this.yAnim.SetBool("Expand", true);
                        this.zAnim.SetBool("Expand", true);
                        this.cAnim.SetBool("Expand", true);
                        this.xyPlane.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    }
                    else
                    {
                        this.xAnim.SetBool("Expand", false);
                        this.yAnim.SetBool("Expand", false);
                        this.zAnim.SetBool("Expand", false);
                        this.cAnim.SetBool("Expand", false);
                        this.xyPlane.transform.localScale = new Vector3(10f, 1f, 10f);
                    }
        //        }
        //    }
        //}
    }
}
