/* CSci-5609 Support Code created by Prof. Dan Keefe, Fall 2023 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IVLab.ABREngine;


/// <summary>
/// A helper class for accessing the raw data values for Line data visualized with ABR.  If there is only
/// one Line Data Impression within the scene, then the class returns data from that Line.  Otherwise,
/// set the LineKeyDataPath property to access a specific Line Data Impression.  ABR Lines come in sets.
/// Given a query point in World or Data Space, this class provides a way to access the closest line and/or
/// the subset of nearby lines.  These methods return indices into the ABR line set.  The class also provides
/// a way to access per-line average data.  For example, after finding the closest line to a data probe,
/// you can access the average value along the whole line for whatever data variable is currently mapped
/// to the line color.
/// </summary>
public class ABRLineDataAccessor : MonoBehaviour
{
    // PUBLIC PROPERTIES

    /// <summary>
    /// This string is not used if there is only one Line DataImpression in the scene.  If there is more than one,
    /// this string must be set to the path of the Line KeyData you wish to access.
    /// </summary>
    public string LineKeyDataPath {
        get {
            return m_LineKeyDataPath;
        }
        set {
            m_LineKeyDataPath = value;
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
    /// Returns the minimum value of the variable currently mapped to the color of the Line.
    /// </summary>
    public float MinColorDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentLineImpression.colorVariable.Range.min;
            } else {
                return OutOfRangeDataValue;
            }
        }
    }

    /// <summary>
    /// Returns the maximum value of the variable currently mapped to the color of the Line.
    /// </summary>
    public float MaxColorDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentLineImpression.colorVariable.Range.max;
            } else {
                return OutOfRangeDataValue;
            }
        }
    }


    /// <summary>
    /// Returns the minimum value of the variable currently mapped to the texture pattern of the Line.
    /// </summary>
    public float MinTextureDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentLineImpression.lineTextureVariable.Range.min;
            } else {
                return OutOfRangeDataValue;
            }
        }
    }

    /// <summary>
    /// Returns the maximum value of the variable currently mapped to the texture pattern of the Line.
    /// </summary>
    public float MaxTextureDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentLineImpression.lineTextureVariable.Range.max;
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
    /// True if the a Line Data Impression is avaiable in the scene to inspect.
    /// </summary>
    /// <returns></returns>
    public bool IsDataAvailable()
    {
        return m_CurrentLineImpression != null;
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
    /// Returns the number of lines in this ABR line dataset
    /// </summary>
    public int GetNumLines()
    {
        SimpleLineRenderInfo renderInfo = m_CurrentLineImpression.RenderInfo as SimpleLineRenderInfo;
        if (renderInfo != null) {
            return renderInfo.vertices.Length;
        } else {
            return 0;
        }
    }

    /// <summary>
    /// Checks all the lines in the dataset and returns the index of the one that comes closest to the supplied point
    /// represented in Unity World coordinates.
    /// </summary>
    public int GetClosestLineInWorldSpace(Vector3 pointInWorldSpace)
    {
        return GetClosestLineInDataSpace(WorldSpacePointToDataSpace(pointInWorldSpace));
    }

    /// <summary>
    /// Checks all the lines in the dataset and returns the index of the one that comes closest to the supplied point
    /// represented in Data Space coordinates.
    /// </summary>
    public int GetClosestLineInDataSpace(Vector3 pointInDataSpace)
    {
        if (IsDataAvailable()) {         
            SimpleLineRenderInfo renderInfo = m_CurrentLineImpression.RenderInfo as SimpleLineRenderInfo;
            if ((renderInfo != null) && (renderInfo.vertices.Length > 0)) {
                int closestLine = 0;
                float closestDist = (pointInDataSpace - renderInfo.vertices[0][0]).magnitude;
                for (int l = 0; l < renderInfo.vertices.Length; l++) {
                    for (int v = 0; v < renderInfo.vertices[l].Length; v++) {
                        float dist = (pointInDataSpace - renderInfo.vertices[l][v]).magnitude;
                        if (dist < closestDist) {
                            closestLine = l;
                            closestDist = dist;
                        }
                    }
                }
                return closestLine;
            }
        }
        return -1;
    }

    /// <summary>
    /// Checks all the lines in the dataset and returns a list of the indices of the lines that come with radius units
    /// of a query point.  Both the query point and the radius are specified in Unity World Space coordinates.
    /// </summary>
    public List<int> GetNearbyLinesInWorldSpace(Vector3 pointInWorldSpace, float radiusInWorldSpace)
    {
        Vector3 radiusVecWorld = new Vector3(radiusInWorldSpace, 0, 0);
        Vector3 radiusVecGroup = m_CurrentDataImpressionGroup.GroupRoot.transform.InverseTransformVector(radiusVecWorld);
        Vector3 radiusVecData = m_CurrentDataImpressionGroup.GroupToDataMatrix *
            new Vector4(radiusVecGroup.x, radiusVecGroup.y, radiusVecGroup.z, 0);

        return GetNearbyLinesInDataSpace(WorldSpacePointToDataSpace(pointInWorldSpace), radiusVecData.magnitude);
    }

    /// <summary>
    /// Checks all the lines in the dataset and returns a list of the indices of the lines that come with radius units
    /// of a query point.  Both the query point and the radius are specified in Data Space coordinates.
    /// </summary>
    public List<int> GetNearbyLinesInDataSpace(Vector3 pointInDataSpace, float radiusInDataSpace)
    {
        List<int> lineIndices = new List<int>();
        if (IsDataAvailable()) {
            SimpleLineRenderInfo renderInfo = m_CurrentLineImpression.RenderInfo as SimpleLineRenderInfo;
            if ((renderInfo != null) && (renderInfo.vertices.Length > 0)) {
                for (int l = 0; l < renderInfo.vertices.Length; l++) {
                    for (int v = 0; v < renderInfo.vertices[l].Length; v++) {
                        float dist = (pointInDataSpace - renderInfo.vertices[l][v]).magnitude;
                        if (dist < radiusInDataSpace) {
                            lineIndices.Add(l);
                            continue;
                        }
                    }
                }
            }
        }
        return lineIndices;
    }

    /// <summary>
    /// Looks up a line by its index, loops through each vertex of the line to look at the data value mapped to color,
    /// and returns the average of these data values.
    /// </summary>
    public float GetAverageColorDataValueOnLine(int lineIndex)
    {
        return GetAverageScalarValueOnLine(lineIndex, 0);
    }

    /// <summary>
    /// Looks up a line by its index, loops through each vertex of the line to look at the data value mapped to texture,
    /// and returns the average of these data values.
    /// </summary>
    public float GetAverageTextureDataValueOnLine(int lineIndex)
    {
        return GetAverageScalarValueOnLine(lineIndex, 1);
    }

    // internal helper
    float GetAverageScalarValueOnLine(int lineIndex, int scalarID)
    {
        if (IsDataAvailable()) {
            SimpleLineRenderInfo renderInfo = m_CurrentLineImpression.RenderInfo as SimpleLineRenderInfo;
            float sum = 0;
            for (int v = 0; v < renderInfo.scalars[lineIndex].Length; v++) {
                sum += renderInfo.scalars[lineIndex][v][scalarID];
            }
            return sum / renderInfo.scalars[lineIndex].Length;
        } else {
            return OutOfRangeDataValue;
        }
    }

    /// <summary>
    /// This function can be used to show/hide a subset of the lines in the set.  Provide a list of the indices of
    /// each line that should be visible.  The rest will be flagged as hidden.
    /// </summary>
    public void SetVisibleLines(List<int> visibleIndices)
    {
        if (IsDataAvailable()) {
            if (m_CurrentLineImpression.RenderHints.PerIndexVisibility == null) {
                m_CurrentLineImpression.RenderHints.PerIndexVisibility = new BitArray(GetNumLines(), true);
            }
            for (int i = 0; i < m_CurrentLineImpression.RenderHints.PerIndexVisibility.Length; i++) {
                m_CurrentLineImpression.RenderHints.PerIndexVisibility[i] = visibleIndices.Contains(i);
            }
            m_CurrentLineImpression.RenderHints.StyleChanged = true;
            ABREngine.Instance.Render();
        }
    }

    /// <summary>
    /// This function resets the visibility to the default of all lines are visible.
    /// </summary>
    public void ResetLineVisibility()
    {
        if (IsDataAvailable()) {
            m_CurrentLineImpression.RenderHints.PerIndexVisibility = null;
            m_CurrentLineImpression.RenderHints.StyleChanged = true;
            ABREngine.Instance.Render();
        }
    }




    /// <summary>
    /// Uses the min and max values in the Line data to remap a data value to a normalized range between 0 and 1.
    /// </summary>
    public float NormalizeColorDataValue(float dataValue)
    {
        return (dataValue - MinColorDataValue) / (MaxColorDataValue - MinColorDataValue);
    }


    /// <summary>
    /// Uses the min and max values in the Line data to remap a data value to a normalized range between 0 and 1.
    /// </summary>
    public float NormalizeTextureDataValue(float dataValue)
    {
        return (dataValue - MinTextureDataValue) / (MaxTextureDataValue - MinTextureDataValue);
    }




    // PRIVATE METHODS

    private void Reset()
    {
        m_LineKeyDataPath = "LANL/FireSim/KeyData/Wind";
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
        m_CurrentLineImpression = null;
        m_CurrentDataImpressionGroup = null;
        m_CurrentRawDataset = null;

        // find all Line data impressions in the scene
        List<SimpleLineDataImpression> allLineImpressions =
            ABREngine.Instance.GetDataImpressions<SimpleLineDataImpression>();

        // If there is only one Line Data Impression in the scene, then use it.
        if (allLineImpressions.Count == 1) {
            m_CurrentLineImpression = allLineImpressions[0];
        }
        // If there is more than one, select based on the key data path
        else if (allLineImpressions.Count > 1) {
            foreach (var vi in allLineImpressions) {
                if (((m_KeyDataPathCanIncludeSuffix) && (vi.GetKeyData().Path.StartsWith(m_LineKeyDataPath))) ||
                    ((!m_KeyDataPathCanIncludeSuffix) && (vi.GetKeyData().Path == m_LineKeyDataPath))) {
                    m_CurrentLineImpression = vi;
                }
            }
            if (m_CurrentLineImpression == null) {
                Debug.LogWarning($"More than one Line Data Impression is available, but none of them are linked to " +
                    "the key data path '{m_LineKeyDataPath}'.  Try setting the LineKeyDataPath property to the " +
                    "path of the Line you would like to access.");
            }
        }

        if (m_CurrentLineImpression != null) {
            m_CurrentDataImpressionGroup = ABREngine.Instance.GetGroupFromImpression(m_CurrentLineImpression);
            ABREngine.Instance.Data.TryGetRawDataset(m_CurrentLineImpression.keyData.Path, out m_CurrentRawDataset);
        }
    }




    // PRIVATE VARIABLES

    // Private variables saved with the scene and shown in the Unity GUI
    [Tooltip("If there is more than one Line data impression in the scene, set this string to key data path of the " +
        "Line to inspect.  This can be changed at runtime."), SerializeField]
    private string m_LineKeyDataPath;

    [Tooltip("If true, then the surface data to access only needs to 'begin with' SurfaceKeyDataPath; this is useful" +
        "when a timestep ID is appended to the path for timevarying data.  If false, the SurfaceKeyDataPath must " +
        "be an exact match with one of the DataImpressions in the scene."), SerializeField]
    private bool m_KeyDataPathCanIncludeSuffix = true;

    [Tooltip("Value returned by GetValueAtWorldPoint() when data are not available/loaded and when requesting data " +
        "at a point that is located outside of the Line."), SerializeField]
    private float m_OutOfRangeDataValue;


    // Runtime-only Private Variables
    private SimpleLineDataImpression m_CurrentLineImpression;
    private DataImpressionGroup m_CurrentDataImpressionGroup;
    private RawDataset m_CurrentRawDataset;


}
