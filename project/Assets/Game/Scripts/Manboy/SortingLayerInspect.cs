using UnityEngine;
using System.Collections;
using AdvancedInspector;

public class SortingLayerInspect : MonoBehaviour {

    [Inspect]
    public string SortingLayerName
    {
        get
        {
            return GetComponent<MeshRenderer>().sortingLayerName;
        }
        set
        {
            GetComponent<MeshRenderer>().sortingLayerName = value;
        }
    }

    [Inspect]
    public int SortingOrder
    {
        get
        {
            return GetComponent<MeshRenderer>().sortingOrder;
        }
        set
        {
            GetComponent<MeshRenderer>().sortingOrder = value;
        }
    }
}
