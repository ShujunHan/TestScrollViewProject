using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopListItme : MonoBehaviour {

    private int index;
    private LoopListContentItem warpContent;

    void OnDestroy()
    {
        warpContent = null;
    }

    public LoopListContentItem WarpContent
    {
        set
        {
            warpContent = value;
        }
    }

    public int Index
    {
        set
        {
            index = value;
            transform.localPosition = warpContent.getLocalPositionByIndex(index);
            gameObject.name = (index < 10) ? ("0" + index) : ("" + index);
            if (warpContent.onInitializeItem != null && index >= 0)
            {
                warpContent.onInitializeItem(gameObject, index);
            }
        }
        get
        {
            return index;
        }
    }


}
