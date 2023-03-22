using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFilter : MonoBehaviour
{
    public float radius;
    public ABRLineDataAccessor lineDataAccessor;

    void reset()
    {
        this.radius = 75;
        lineDataAccessor = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 probePosInWorldSpace = gameObject.transform.position;
        var nearbyLines = this.lineDataAccessor.GetNearbyLinesInWorldSpace(probePosInWorldSpace, radius);
        this.lineDataAccessor.SetVisibleLines(nearbyLines);
    }
}
