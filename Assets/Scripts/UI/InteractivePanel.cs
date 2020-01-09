using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractivePanel : MonoBehaviour
{
    float tmpHeight;
    RectTransform rt;

    // Start is called before the first frame update
    void Start()
    {
        rt = this.GetComponent(typeof(RectTransform)) as RectTransform;
        tmpHeight = 0;
    }

    public void SetPanelSize(float h)
    {
        if (h > tmpHeight)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, h);
            tmpHeight = h;
        }
        
        Debug.Log(">>>>>." + h);
    }

    public void DeletePressed()
    {

    }
}
