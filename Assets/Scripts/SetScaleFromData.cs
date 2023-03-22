/* CSci-5609 Support Code created by Prof. Dan Keefe, Fall 2023 */

using UnityEngine;


/// <summary>
/// Attach this script to a GameObject to easily scale the object in response to data.  To make the object
/// scale uniformly in the X, Y, and Z directions, the x,y,z values for minScale should all be the same, and
/// the x,y,z values for maxScale should all be the same.  To just change the scale in a single dimension,
/// make minScale and maxScale differ in only that dimension.
/// </summary>
public class SetScaleFromData : MonoBehaviour
{
    [Tooltip("Drag a GameObject here to set the object you wish to scale, or leave unset to default to " +
        "scaling the object this script is attached to.")]
    public Transform objectToScale;

    [Tooltip("The scale factors in the X, Y, and Z dimensions to apply when the data value is <= minDataValue")]
    public Vector3 minScale;

    [Tooltip("The scale factors in the X, Y, and Z dimensions to apply when the data value is >= maxDataValue")]
    public Vector3 maxScale;

    [Tooltip("The data value at which the minScale should be applied.")]
    public float minDataValue;

    [Tooltip("The data value at which the maxScale should be applied.")]
    public float maxDataValue;

    [Tooltip("The current data value used to set the scale.  You can set a default/initial value for " +
        "this via the Editor.  Then, change the value at  runtime by calling SetDataValue(), and " +
        "the object's scale will change in response.")]
    public float dataValue;

    /// <summary>
    /// Call this at runtime to set the 'current data value' and adjust the object's scale in response.
    /// </summary>
    public void SetDataValue(float newValue)
    {
        dataValue = newValue;
    }

    private void Reset()
    {
        minScale = new Vector3(1, 1, 1);
        maxScale = new Vector3(10, 10, 10);
        minDataValue = 0;
        maxDataValue = 1;
        dataValue = 0;
    }

    private void Start()
    {
        // if a different transform was not set in the editor, then scale the transform for the gameobject
        // this script is attached to.
        if (objectToScale == null) {
            objectToScale = transform;
        }
    }

    void Update()
    {
        float amt = Mathf.Clamp((dataValue - minDataValue) / (maxDataValue - minDataValue), 0, 1);
        Vector3 newScale = Vector3.Lerp(minScale, maxScale, amt);
        objectToScale.localScale = newScale;
    }
}
