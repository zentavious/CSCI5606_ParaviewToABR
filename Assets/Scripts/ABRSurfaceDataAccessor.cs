/* CSci-5609 Support Code created by Prof. Dan Keefe, Fall 2023 */

using System.Collections.Generic;
using UnityEngine;

using IVLab.ABREngine;


/// <summary>
/// A helper class for accessing the raw data values for Surface data visualized with ABR.  If there is only
/// one Surface Data Impression within the scene, then the class returns data from that Surface.  Otherwise,
/// set the SurfaceKeyDataPath property to access a specific Surface Data Impression.  ABR Surfaces can encode
/// data using color or a texture pattern.  This class provides access to either of those encodings (i.e., you
/// can access the underlying data for whatever variable is currently mapped to the color or pattern).
/// </summary>
public class ABRSurfaceDataAccessor : MonoBehaviour
{
    // PUBLIC PROPERTIES

    /// <summary>
    /// This string is not used if there is only one Surface DataImpression in the scene.  If there is more than one,
    /// this string must be set to the path of the Surface KeyData you wish to access.
    /// </summary>
    public string SurfaceKeyDataPath {
        get {
            return m_SurfaceKeyDataPath;
        }
        set {
            m_SurfaceKeyDataPath = value;
            UpdateCurrentDataImpression();
        }
    }

    /// <summary>
    /// This value is returned when a data lookup fails because no data are available.
    /// </summary>
    public float OutOfRangeDataValue {
        get {
            return m_OutOfRangeDataValue;
        }
        set {
            m_OutOfRangeDataValue = value;
        }
    }

    /// <summary>
    /// Returns the minimum value of the variable currently mapped to the color of the surface.
    /// </summary>
    public float MinColorDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentSurfaceImpression.colorVariable.Range.min;
            } else {
                return OutOfRangeDataValue;
            }
        }
    }

    /// <summary>
    /// Returns the maximum value of the variable currently mapped to the color of the surface.
    /// </summary>
    public float MaxColorDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentSurfaceImpression.colorVariable.Range.max;
            } else {
                return OutOfRangeDataValue;
            }
        }
    }


    /// <summary>
    /// Returns the minimum value of the variable currently mapped to the texture pattern of the surface.
    /// </summary>
    public float MinPatternDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentSurfaceImpression.patternVariable.Range.min;
            } else {
                return OutOfRangeDataValue;
            }
        }
    }

    /// <summary>
    /// Returns the maximum value of the variable currently mapped to the texture pattern of the surface.
    /// </summary>
    public float MaxPatternDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentSurfaceImpression.patternVariable.Range.max;
            } else {
                return OutOfRangeDataValue;
            }
        }
    }

    /// <summary>
    /// Returns the spatial bounding box of the data in the original coordinate system of the data.  Data
    /// Space coordinates are typically based upon real world units, like meters.
    /// </summary>
    public Bounds BoundsInDataSpace {
        get {
            if (IsDataAvailable()) {
                return m_CurrentRawDataset.bounds;
            } else {
                return new Bounds();
            }
        }
    }




    // PUBLIC METHODS

    /// <summary>
    /// True if the Surface Data Impression is avaiable for data access.
    /// </summary>
    /// <returns></returns>
    public bool IsDataAvailable()
    {
        return m_CurrentSurfaceImpression != null;
    }

    /// <summary>
    /// Converts a point in Unity world coordinates to a point within the original data coordinate space.
    /// The data coordinate space is typically defined in real-world units, like cm or meters.  Those
    /// coordinates are often scaled or repositioned within Unity World space as we zoom into the data or
    /// place multiple datasets side-by-side or do other visualization tasks.
    /// </summary>
    public Vector3 WorldSpacePointToDataSpace(Vector3 pointInWorldSpace)
    {
        if (IsDataAvailable()) {
            Vector3 pointInGroupSpace =
                m_CurrentDataImpressionGroup.GroupRoot.transform.InverseTransformPoint(pointInWorldSpace);
            Vector3 pointInDataSpace = m_CurrentDataImpressionGroup.GroupToDataMatrix * pointInGroupSpace;
            return pointInDataSpace;
        } else {
            return pointInWorldSpace;
        }
    }

    /// <summary>
    /// Converts a point in the original data coordinate space, which is typically defined in real-world
    /// units, like meters, to its current position in Unity's World coordinate system, which might include
    /// a scale or translation or other transformation based on how the visualization is designed.
    /// </summary>
    public Vector3 DataSpacePointToWorldSpace(Vector3 pointInDataSpace)
    {
        if (IsDataAvailable()) {
            Vector3 pointInGroupSpace = m_CurrentDataImpressionGroup.GroupToDataMatrix.inverse * pointInDataSpace;
            Vector3 pointInWorldSpace = m_CurrentDataImpressionGroup.GroupRoot.transform.InverseTransformPoint(pointInGroupSpace);
            return pointInWorldSpace;
        } else {
            return pointInDataSpace;
        }
    }


    /// <summary>
    /// Returns true if the point in Unity World coordinates lies within the bounds of the data.
    /// </summary>
    public bool ContainsWorldSpacePoint(Vector3 pointInWorldSpace)
    {
        if (IsDataAvailable()) {
            return BoundsInDataSpace.Contains(WorldSpacePointToDataSpace(pointInWorldSpace));
        } else {
            return false;
        }
    }

    /// <summary>
    /// Returns true if the point in data coordinates lies within the volume. Data coordinates are typically
    /// defined in real-world units, like meters.
    /// </summary>
    public bool ContainsDataSpacePoint(Vector3 pointInDataSpace)
    {
        if (IsDataAvailable()) {
            return BoundsInDataSpace.Contains(pointInDataSpace);
        } else {
            return false;
        }
    }


    /// <summary>
    /// Finds the closest vertex on the surface and returns the value of the data currently mapped to color
    /// at that point.
    /// </summary>
    public float GetColorValueAtClosestWorldSpacePoint(Vector3 pointInWorldSpace)
    {
        Vector3 pointInDataSpace = WorldSpacePointToDataSpace(pointInWorldSpace);
        return GetColorValueAtClosestDataSpacePoint(pointInDataSpace);
    }

    /// <summary>
    /// Finds the closest vertex on the surface and returns the value of the data currently mapped to color
    /// at that point.
    /// </summary>
    public float GetColorValueAtClosestDataSpacePoint(Vector3 pointInDataSpace)
    {
        return GetScalarValueAtClosestDataSpacePoint(pointInDataSpace, 0);
    }


    /// <summary>
    /// Finds the closest vertex on the surface and returns the value of the data currently mapped to the
    /// texture pattern at that point.
    /// </summary>
    public float GetPatternValueAtClosestWorldSpacePoint(Vector3 pointInWorldSpace)
    {
        Vector3 pointInDataSpace = WorldSpacePointToDataSpace(pointInWorldSpace);
        return GetPatternValueAtClosestDataSpacePoint(pointInDataSpace);
    }

    /// <summary>
    /// Finds the closest vertex on the surface and returns the value of the data currently mapped to the
    /// texture pattern at that point.
    /// </summary>
    public float GetPatternValueAtClosestDataSpacePoint(Vector3 pointInDataSpace)
    {
        return GetScalarValueAtClosestDataSpacePoint(pointInDataSpace, 1);
    }


    // internal helper function
    float GetScalarValueAtClosestDataSpacePoint(Vector3 pointInDataSpace, int scalarID)
    {
        if (IsDataAvailable()) {
            SimpleSurfaceRenderInfo renderInfo = m_CurrentSurfaceImpression.RenderInfo as SimpleSurfaceRenderInfo;
            if ((renderInfo != null) && (renderInfo.vertices.Length > 0)) {
                int closestI = 0;
                float closestDist = (pointInDataSpace - renderInfo.vertices[0]).magnitude;
                for (int i = 1; i < renderInfo.vertices.Length; i++) {
                    float dist = (pointInDataSpace - renderInfo.vertices[i]).magnitude;
                    if (dist < closestDist) {
                        closestI = i;
                        closestDist = dist;
                    }
                }
                return renderInfo.scalars[closestI][scalarID];
            }
        }
        return OutOfRangeDataValue;
    }

    /// <summary>
    /// The surface data are drawn using a Unity GameObject with a Mesh Renderer attached.  This function
    /// provides access to the GameObject responsible for drawing the surface data.
    /// </summary>
    public GameObject GetGameObject()
    {
        if (IsDataAvailable()) {
            return ABREngine.Instance.GetEncodedGameObject(m_CurrentSurfaceImpression.Uuid).gameObject;
        } else {
            return null;
        }
    }

    /// <summary>
    /// The surface data are drawn using a Unity GameObject with a Mesh Renderer attached.  This function
    /// provides access to the Mesh responsible for drawing the surface data.
    /// </summary>
    public Mesh GetMesh()
    {
        GameObject go = GetGameObject();
        if (go != null) {
            return go.GetComponent<Mesh>();
        } else {
            return null;
        }
    }

    /// <summary>
    /// The surface data are drawn using a Unity GameObject with a Mesh Renderer attached.  This function
    /// provides access to a MeshCollider (if any) attached to that GameObject.
    /// </summary>
    public MeshCollider GetMeshCollider()
    {
        GameObject go = GetGameObject();
        if (go != null) {
            return go.GetComponent<MeshCollider>();
        } else {
            return null;
        }
    }



    /// <summary>
    /// Uses the min and max values in the Surface data to remap a data value to a normalized range between 0 and 1.
    /// </summary>
    public float NormalizeColorDataValue(float dataValue)
    {
        return (dataValue - MinColorDataValue) / (MaxColorDataValue - MinColorDataValue);
    }


    /// <summary>
    /// Uses the min and max values in the Surface data to remap a data value to a normalized range between 0 and 1.
    /// </summary>
    public float NormalizePatternDataValue(float dataValue)
    {
        return (dataValue - MinPatternDataValue) / (MaxPatternDataValue - MinPatternDataValue);
    }




    // PRIVATE METHODS

    private void Reset()
    {
        m_SurfaceKeyDataPath = "LANL/FireSim/KeyData/Ground";
        m_KeyDataPathCanIncludeSuffix = true;
        m_OutOfRangeDataValue = -9999;
    }

    void Update()
    {
        // This line intentionally (re)finds the appropriate data impression each frame since the available data
        // impressions may change at runtime (e.g., as designers work with ABR Compose).
        UpdateCurrentDataImpression();
    }

    void UpdateCurrentDataImpression()
    {
        // reset these cached variables
        m_CurrentSurfaceImpression = null;
        m_CurrentDataImpressionGroup = null;
        m_CurrentRawDataset = null;

        // find all Surface data impressions in the scene
        List<SimpleSurfaceDataImpression> allSurfaceImpressions =
            ABREngine.Instance.GetDataImpressions<SimpleSurfaceDataImpression>();

        // If there is only one Surface Data Impression in the scene, then use it.
        if (allSurfaceImpressions.Count == 1) {
            m_CurrentSurfaceImpression = allSurfaceImpressions[0];
        }
        // If there is more than one, select based on the key data path
        else if (allSurfaceImpressions.Count > 1) {
            foreach (var vi in allSurfaceImpressions) {
                Debug.LogWarning(vi.GetKeyData().Path);
                if (((m_KeyDataPathCanIncludeSuffix) && (vi.GetKeyData().Path.StartsWith(m_SurfaceKeyDataPath))) ||
                    ((!m_KeyDataPathCanIncludeSuffix) && (vi.GetKeyData().Path == m_SurfaceKeyDataPath))) {
                    m_CurrentSurfaceImpression = vi;
                }
            }
            if (m_CurrentSurfaceImpression == null) {
                Debug.LogWarning("More than one Surface Data Impression is available, but none of them are linked to " +
                    $"the key data path '{m_SurfaceKeyDataPath}'.  Try setting the SurfaceKeyDataPath property to the " +
                    "path of the Surface you would like to access.");
            }
        }

        if (m_CurrentSurfaceImpression != null) {
            m_CurrentDataImpressionGroup = ABREngine.Instance.GetGroupFromImpression(m_CurrentSurfaceImpression);
            ABREngine.Instance.Data.TryGetRawDataset(m_CurrentSurfaceImpression.keyData.Path, out m_CurrentRawDataset);
        }
    }




    // PRIVATE VARIABLES

    // Private variables saved with the scene and shown in the Unity GUI
    [Tooltip("If there is more than one Surface data impression in the scene, set this string to key data path of the " +
        "Surface to inspect.  This can be changed at runtime."), SerializeField]
    private string m_SurfaceKeyDataPath;

    [Tooltip("If true, then the surface data to access only needs to 'begin with' SurfaceKeyDataPath; this is useful" +
        "when a timestep ID is appended to the path for timevarying data.  If false, the SurfaceKeyDataPath must " +
        "be an exact match with one of the DataImpressions in the scene."), SerializeField]
    private bool m_KeyDataPathCanIncludeSuffix = true;


    [Tooltip("Value returned by GetValueAtWorldPoint() when data are not available/loaded and when requesting data " +
        "at a point that is located outside of the Surface."), SerializeField]
    private float m_OutOfRangeDataValue;

    // Runtime-only Private Variables
    private SimpleSurfaceDataImpression m_CurrentSurfaceImpression;
    private DataImpressionGroup m_CurrentDataImpressionGroup;
    private RawDataset m_CurrentRawDataset;
}
