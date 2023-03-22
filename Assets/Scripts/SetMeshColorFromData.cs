/* CSci-5609 Support Code created by Prof. Dan Keefe, Fall 2023 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Attach to a GameObject that renders a Mesh to be able to easily change the color of that mesh via a
/// color map.  Call SetDataValue() at runtime to do the color map lookup and change the mesh's color.
/// </summary>
public class SetMeshColorFromData : MonoBehaviour
{
    [Tooltip("Drag a GameObject that has a ColorMap component on it here, or leave it unset if this " +
        "GameObject has a colormap attached directly to it.")]
    public ColorMap colorMap;

    [Tooltip("Drag a GameObject that has a MeshRenderer component on it here, or leave it unset if this " +
        "GameObject has a MeshRenderer attached directly to it.")]
    public MeshRenderer meshRenderer;

    [Tooltip("The current data value used to lookup a color in the color map.  You can set a default/initial " +
        "value for this via the Editor.  Then, change the value at  runtime by calling SetDataValue(), and " +
        "the script will update the mesh's color in response.")]
    public float dataValue;


    /// <summary>
	/// After setting a new data value, the Mesh's material will update its color on the next Update()
	/// </summary>
    public void SetDataValue(float newValue)
    {
        dataValue = newValue;
    }


    private void Start()
    {
        // if not already set in the editor, look for a color map attached to the same gameobject
        if (colorMap == null) {
            colorMap = GetComponent<ColorMap>();
        }
        if (colorMap == null) {
            Debug.LogWarning("Missing a Color Map");
        }

        // if not already set in the editor, look for a mesh renderer attached to the this gameobject or its parent
        if (meshRenderer == null) {
            meshRenderer = GetComponentInParent<MeshRenderer>();
        }
        if (meshRenderer == null) {
            Debug.LogWarning("Missing a Mesh Renderer");
        }
    }

    void Update()
    {
        if ((material == null) && (meshRenderer != null))
        {
            material = meshRenderer.material;
        }

        if ((meshRenderer != null) && (colorMap != null)) {
            material.color = colorMap.LookupColor(dataValue);
        }
    }

    // private runtime only variables
    Material material;
}
