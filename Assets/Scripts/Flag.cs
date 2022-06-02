using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flag : MonoBehaviour {

    public Color colFlagged;
    public Color colUnflagged;
    public Color colVisible;

    public Image imgFlag;

    public bool bFlagged;
    public bool bVisible;



    public void SetVisible() {
        bVisible = true;
        UpdateVisual();
    }

    public void UnsetVisible() {
        bVisible = false;
        UpdateVisual();
    }

    public void OnClickFlag() {
        if(bFlagged) UnsetFlag();
        else SetFlag();
    }


    public void SetFlag() {
        bFlagged = true;
        UpdateVisual();
    }

    public void UnsetFlag() {
        bFlagged = false;
        UpdateVisual();
    }

    public void UpdateVisual() {

        if(bFlagged) {
            imgFlag.color = colFlagged;
        } else if(bVisible) {
            imgFlag.color = colVisible;
        } else {
            imgFlag.color = colUnflagged;
        }

    }

    public void Start() {
        UpdateVisual();
    }
}
