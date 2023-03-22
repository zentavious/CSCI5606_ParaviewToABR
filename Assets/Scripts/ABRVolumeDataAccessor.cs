/* CSci-5609 Support Code created by Prof. Dan Keefe, Fall 2023 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IVLab.ABREngine;


/// <summary>
/// A helper class for accessing the raw data values for volume data visualized with ABR.  If there is only
/// one Volume Data Impression within the scene, then the class returns data from that volume.  Otherwise,
/// set the VolumeKeyDataPath property to access a specific Volume Data Impression.
/// </summary>
public class ABRVolumeDataAccessor : MonoBehaviour
{
    // PUBLIC PROPERTIES

    /// <summary>
    /// This string is not used if there is only one Volume DataImpression in the scene.  If there is more than one,
    /// this string must be set to the path of the Volume KeyData you wish to access.
    /// </summary>
    public string VolumeKeyDataPath {
        get {
            return m_VolumeKeyDataPath;
        }
        set {
            m_VolumeKeyDataPath = value;
            UpdateCurrentDataImpression();
        }
    }

    /// <summary>
    /// This value can be used like a flag for a failed GetValueAtWorldPoint() query, for example, when the world
    /// point falls outside of the bounds of the dataset.
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
    /// The minimum value in the current volume data -- read only.
    /// </summary>
    public float MinDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentVolumeImpression.colorVariable.Range.min;
            } else {
                return OutOfRangeDataValue;
            }
        }
    }

    /// <summary>
    /// The maximum value in the current volume data -- read only.
    /// </summary>
    public float MaxDataValue {
        get {
            if (IsDataAvailable()) {
                return m_CurrentVolumeImpression.colorVariable.Range.max;
            } else {
                return OutOfRangeDataValue;
            }
        }
    }

    /// <summary>
    /// The spatial bounding box of the data in the original coordinate system of the data -- read only.
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

    /// <summary>
    /// The x,y,z dimensions of the raw voxel data (i.e., the number of voxels in each direction) -- read only.
    /// </summary>
    public Vector3Int VolumeDimensionsInVoxels {
        get {
            if (IsDataAvailable()) {
                return m_CurrentRawDataset.dimensions;
            } else {
                return new Vector3Int(0, 0, 0);
            }
        }
    }



    // PUBLIC METHODS

    /// <summary>
    /// True if the Volume Data Impression is avaiable in the scene to inspect.
    /// </summary>
    /// <returns></returns>
    public bool IsDataAvailable()
    {
        return m_CurrentVolumeImpression != null;
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
    /// Converts a point in Unity world coordinates to a point within the data coordinate space and then
    /// to a corresponding point in voxel coordinate space.  The voxel space is defined in units of voxels,
    /// but the coordinates can be fractional so we can represent a point within a voxel, not just at the
    /// center of each voxel.
    /// </summary>
    public Vector3 WorldSpacePointToVoxelSpace(Vector3 pointInWorldSpace)
    {
        Vector3 pointInDataSpace = WorldSpacePointToDataSpace(pointInWorldSpace);
        return DataSpacePointToVoxelSpace(pointInDataSpace);
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
    /// Converts a point in the original data coordinate space, which is typically defined in real-world
    /// units, like meters, to a point within the voxel coordinate space.  The voxel space is defined in
    /// units of voxels, but the coordinates can be fractional so we can represent a point within a voxel,
    /// not just at the center of each voxel.
    /// </summary>
    public Vector3 DataSpacePointToVoxelSpace(Vector3 pointInDataSpace)
    {
        if (IsDataAvailable()) {
            Vector3 pointInNormalizedDataSpace = new Vector3(
                (pointInDataSpace.x - BoundsInDataSpace.min.x) / BoundsInDataSpace.size.x,
                (pointInDataSpace.y - BoundsInDataSpace.min.y) / BoundsInDataSpace.size.y,
                (pointInDataSpace.z - BoundsInDataSpace.min.z) / BoundsInDataSpace.size.z);

            return new Vector3(
                pointInNormalizedDataSpace.x * VolumeDimensionsInVoxels.x,
                pointInNormalizedDataSpace.y * VolumeDimensionsInVoxels.y,
                pointInNormalizedDataSpace.z * VolumeDimensionsInVoxels.z);
        } else {
            return pointInDataSpace;
        }
    }


    /// <summary>
    /// Converts a point in voxel space to the data coordinate space.  Typically this transforms the
    /// voxels, which are like pixels in an image, into a real-world coordinate space like meters.
    /// </summary>
    public Vector3 VoxelSpacePointToDataSpace(Vector3 pointInVoxelSpace)
    {
        Vector3 pointInNormalizedDataSpace = new Vector3(
            pointInVoxelSpace.x / (float)VolumeDimensionsInVoxels.x,
            pointInVoxelSpace.y / (float)VolumeDimensionsInVoxels.y,
            pointInVoxelSpace.z / (float)VolumeDimensionsInVoxels.z);

        Vector3 pointInDataSpace = new Vector3(
            BoundsInDataSpace.min.x + pointInNormalizedDataSpace.x * BoundsInDataSpace.size.x,
            BoundsInDataSpace.min.y + pointInNormalizedDataSpace.x * BoundsInDataSpace.size.y,
            BoundsInDataSpace.min.z + pointInNormalizedDataSpace.x * BoundsInDataSpace.size.z);

        return pointInDataSpace;
    }

    /// <summary>
    /// Converts a point in voxel space (can include fractions) to the point's current position in
    /// Unity's World coordinate system.
    /// </summary>
    public Vector3 VoxelSpacePointToWorldSpace(Vector3 pointInVoxelSpace)
    {
        Vector3 pointInDataSpace = VoxelSpacePointToDataSpace(pointInVoxelSpace);
        return DataSpacePointToWorldSpace(pointInDataSpace);
    }



    /// <summary>
    /// Returns true if the point in Unity World coordinates lies within the volume.
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
    /// Returns true if the point in voxel space lies within the volume.  Since voxel space is defined
    /// in units of voxels, this simply checks to see if the point lies within the dimensions of the volume.
    /// </summary>
    public bool ContainsVoxelSpacePoint(Vector3 pointInVoxelSpace)
    {
        if (IsDataAvailable()) {
            return (pointInVoxelSpace.x >= 0) && (pointInVoxelSpace.x < VolumeDimensionsInVoxels.x) &&
                (pointInVoxelSpace.y >= 0) && (pointInVoxelSpace.y < VolumeDimensionsInVoxels.y) &&
                (pointInVoxelSpace.z >= 0) && (pointInVoxelSpace.z < VolumeDimensionsInVoxels.z);
        } else {
            return false;
        }
    }

    /// <summary>
    /// Looks up the value in the original voxel data based upon a position specified in Unity World coordinates.
    /// </summary>
    public float GetValueAtWorldSpacePoint(Vector3 pointInWorldSpace)
    {
        Vector3 pointInVoxelSpace = WorldSpacePointToVoxelSpace(pointInWorldSpace);
        return GetValueAtVoxelSpacePoint(pointInVoxelSpace);
    }

    /// <summary>
    /// Looks up the value in the original voxel data based upon a position specified in data space coordinates
    /// (i.e., real-world units).
    /// </summary>
    public float GetValueAtDataSpacePoint(Vector3 pointInDataSpace)
    {
        Vector3 pointInVoxelSpace = DataSpacePointToVoxelSpace(pointInDataSpace);
        return GetValueAtVoxelSpacePoint(pointInVoxelSpace);
    }

    /// <summary>
    /// Looks up the value in the original voxel data based upon a point in voxel space.  In the raw volume data
    /// each voxel contains one data value.  If we think of those being the values of the data at a point right
    /// in the center of each voxel, then the point we are querying will rarely align perfectly with those center
    /// points; rather, it will fall somewhere between them.  That is why the pointInVoxelSpace is allowed to
    /// include a fractional component.  This function uses the fractional portion to perform a tri-linear
    /// interpolation of the data from the eight surrounding voxels to estimate the data value at the exact
    /// 3D location requested.  See also GetValueAtNearestVoxel(), which is faster but less precise because it
    /// does not use interpolation.  Or, GetValueAtVoxel(), which is even faster but does not support fractional
    /// voxel coordinates.
    /// </summary>
    public float GetValueAtVoxelSpacePoint(Vector3 pointInVoxelSpace)
    {
        int xFloor = Mathf.Max(Mathf.FloorToInt(pointInVoxelSpace.x), 0);
        int xCeil = Mathf.Min(Mathf.CeilToInt(pointInVoxelSpace.x), VolumeDimensionsInVoxels.x - 1);
        int yFloor = Mathf.Max(Mathf.FloorToInt(pointInVoxelSpace.y), 0);
        int yCeil = Mathf.Min(Mathf.CeilToInt(pointInVoxelSpace.y), VolumeDimensionsInVoxels.y - 1);
        int zFloor = Mathf.Max(Mathf.FloorToInt(pointInVoxelSpace.z), 0);
        int zCeil = Mathf.Min(Mathf.CeilToInt(pointInVoxelSpace.z), VolumeDimensionsInVoxels.z - 1);

        // Lookup the data values at the centers of the 8 voxels surrounding pointInVoxelSpace.
        float v000 = GetValueAtVoxel(new Vector3Int(xFloor, yFloor, zFloor));
        float v100 = GetValueAtVoxel(new Vector3Int(xCeil,  yFloor, zFloor));
        float v010 = GetValueAtVoxel(new Vector3Int(xFloor, yCeil,  zFloor));
        float v110 = GetValueAtVoxel(new Vector3Int(xCeil,  yCeil,  zFloor));
        float v001 = GetValueAtVoxel(new Vector3Int(xCeil,  yFloor, zCeil));
        float v101 = GetValueAtVoxel(new Vector3Int(xFloor, yFloor, zCeil));
        float v011 = GetValueAtVoxel(new Vector3Int(xCeil,  yCeil,  zCeil));
        float v111 = GetValueAtVoxel(new Vector3Int(xFloor, yCeil,  zCeil));

        // Treat the centers of those surrounding voxels as defining the vertices of a cube that surrounds
        // pointInVoxelSpace.  Given, the 8 values at the corners of the cube, use tri-linear interpolation
        // to find the interpolated value at pointInVoxelSpace

        // 1. interpolate across x dim of the cube, to find the values at 4 points that lie on a plane
        // parallel to the XZ plane and that passes through pointInVoxelSpace
        float xFrac = Mathf.Max(pointInVoxelSpace.x - xFloor, 0);
        float vx00 = v000 + xFrac * (v100 - v000); // the y=0, z=0 edge
        float vx01 = v001 + xFrac * (v101 - v001); // the y=0, z=1 edge        
        float vx10 = v010 + xFrac * (v110 - v010); // the y=1, z=0 edge
        float vx11 = v011 + xFrac * (v111 - v011); // the y=1, z=1 edge

        // 2. interpolate in depth (z) to find the values at two points within the plane in step 1.
        // these two points lie on a vertical line segment that also passes through pointInVoxelSpace
        float zFrac = Mathf.Max(pointInVoxelSpace.z - zFloor, 0);
        float vx0z = vx00 + zFrac * (vx01 - vx00); // in the y=0 plane
        float vx1z = vx10 + zFrac * (vx11 - vx10); // in the y=1 plane

        // 3. interpolate in height (y) to find the value at pointInVoxelSpace, which is partway between
        // the two points in step 2.
        float yFrac = Mathf.Max(pointInVoxelSpace.y - yFloor, 0);
        float vxyz = vx0z + yFrac * (vx1z - vx0z); // parallel to the y axis

        return vxyz;
    }

    /// <summary>
    /// Looks up the value in the original voxel data based upon a point in voxel space.  This function does
    /// not use interpolation.  It simply returns that data value stored for the center point of the closest
    /// voxel.
    /// </summary>
    public float GetValueAtNearestVoxel(Vector3 pointInVoxelSpace) {
        float dataValue = m_OutOfRangeDataValue;
        if ((IsDataAvailable()) && (ContainsVoxelSpacePoint(pointInVoxelSpace))) {
            Vector3Int nearsetVoxel = new Vector3Int(
                Mathf.Clamp(Mathf.RoundToInt(pointInVoxelSpace.x), 0, VolumeDimensionsInVoxels.x - 1),
                Mathf.Clamp(Mathf.RoundToInt(pointInVoxelSpace.y), 0, VolumeDimensionsInVoxels.y - 1),
                Mathf.Clamp(Mathf.RoundToInt(pointInVoxelSpace.z), 0, VolumeDimensionsInVoxels.z - 1));
            dataValue = GetValueAtVoxel(nearsetVoxel);
        }
        return dataValue;
    }

    /// <summary>
    /// Looks up the value in the original voxel data based upon its integer voxel coordinates.
    /// </summary>
    public float GetValueAtVoxel(Vector3Int voxelCoords) {
        if (IsDataAvailable()) {
            int x = voxelCoords.x;
            int y = voxelCoords.y;
            int z = voxelCoords.z;

            int voxelIndex = x + y * VolumeDimensionsInVoxels.x + z * VolumeDimensionsInVoxels.x * VolumeDimensionsInVoxels.y;

            float[] dataArray = m_CurrentVolumeImpression.colorVariable.GetArray(m_CurrentVolumeImpression.keyData);
            if ((voxelIndex >= 0) && (voxelIndex < dataArray.Length)) {
                return dataArray[voxelIndex];
            }
        }
        return m_OutOfRangeDataValue;
    }


    /// <summary>
    /// This function can be used to control the visibility of individual voxels.  This is useful, for example,
    /// when implementing data filtering techniques, where you may wish to show only voxels with data values
    /// that lie within a certain range.  For efficiency, the updated visibility flags are not sent to the
    /// graphics card until you call ApplyVoxelVisibility().  Call SetVoxelVisibility() as many times as needed
    /// first, then call ApplyVoxelVisibility() once to save the updated visibility flags to the graphics card.
    /// </summary>
    public void SetVoxelVisibility(int x, int y, int z, bool show)
    {
        if (IsDataAvailable()) {
            if (m_CurrentVolumeImpression.RenderHints.PerIndexVisibility == null) {
                int len = m_CurrentVolumeImpression.colorVariable.GetArray(m_CurrentVolumeImpression.keyData).Length;
                m_CurrentVolumeImpression.RenderHints.PerIndexVisibility = new BitArray(len, true);
            }

            int voxelIndex = x + y * m_CurrentRawDataset.dimensions.x + z * m_CurrentRawDataset.dimensions.x * m_CurrentRawDataset.dimensions.y;
            m_CurrentVolumeImpression.RenderHints.PerIndexVisibility[voxelIndex] = show;
        }
    }

    /// <summary>
    /// This function can be used to control the visibility of individual voxels.  This is useful, for example,
    /// when implementing data filtering techniques, where you may wish to show only voxels with data values
    /// that lie within a certain range.  For efficiency, the updated visibility flags are not sent to the
    /// graphics card until you call ApplyVoxelVisibility().  Call SetVoxelVisibility() as many times as needed
    /// first, then call ApplyVoxelVisibility() once to save the updated visibility flags to the graphics card.
    /// </summary>
    public void ApplyVoxelVisibility()
    {
        if (IsDataAvailable()) {
            m_CurrentVolumeImpression.RenderHints.StyleChanged = true;
            ABREngine.Instance.Render();
        }
    }

    /// <summary>
    /// This function can be used to control the visibility of individual voxels.  This is useful, for example,
    /// when implementing data filtering techniques, where you may wish to show only voxels with data values
    /// that lie within a certain range.  This function resets the visibility to the default, which is that all
    /// voxels are visible.
    /// </summary>
    public void ResetVoxelVisibility()
    {
        if (IsDataAvailable()) {
            m_CurrentVolumeImpression.RenderHints.PerIndexVisibility = null;
            m_CurrentVolumeImpression.RenderHints.StyleChanged = true;
            ABREngine.Instance.Render();
        }
    }



    /// <summary>
    /// Uses the min and max values in the volume data to remap a data value to a normalized range between 0 and 1.
    /// </summary>
    public float NormalizeDataValue(float dataValue)
    {
        return (dataValue - MinDataValue) / (MaxDataValue - MinDataValue);
    }


    // PRIVATE METHODS

    private void Reset()
    {
        m_VolumeKeyDataPath = "LANL/FireSim/KeyData/Smoke";
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
        m_CurrentVolumeImpression = null;
        m_CurrentDataImpressionGroup = null;
        m_CurrentRawDataset = null;

        // find all volume data impressions in the scene
        List<SimpleVolumeDataImpression> allVolumeImpressions =
            ABREngine.Instance.GetDataImpressions<SimpleVolumeDataImpression>();
        
        // If there is only one Volume Data Impression in the scene, then use it.
        if (allVolumeImpressions.Count == 1) {
            m_CurrentVolumeImpression = allVolumeImpressions[0];
        }
        // If there is more than one, select based on the key data path
        else if (allVolumeImpressions.Count > 1) {
            foreach (var vi in allVolumeImpressions) {
                if (((m_KeyDataPathCanIncludeSuffix) && (vi.GetKeyData().Path.StartsWith(m_VolumeKeyDataPath))) ||
                    ((!m_KeyDataPathCanIncludeSuffix) && (vi.GetKeyData().Path == m_VolumeKeyDataPath))) {
                    m_CurrentVolumeImpression = vi;
                }
            }
            if (m_CurrentVolumeImpression == null) {
                Debug.LogWarning($"More than one Volume Data Impression is available, but none of them are linked to " +
                    "the key data path '{m_VolumeKeyDataPath}'.  Try setting the VolumeKeyDataPath property to the " +
                    "path of the volume you would like to access.");
            }
        }

        if (m_CurrentVolumeImpression != null) {
            m_CurrentDataImpressionGroup = ABREngine.Instance.GetGroupFromImpression(m_CurrentVolumeImpression);
            ABREngine.Instance.Data.TryGetRawDataset(m_CurrentVolumeImpression.keyData.Path, out m_CurrentRawDataset);
        }
    }

   


    // PRIVATE VARIABLES

    // Private variables saved with the scene and shown in the Unity GUI
    [Tooltip("If there is more than one volume data impression in the scene, set this string to key data path of the " +
        "volume to inspect.  This can be changed at runtime."), SerializeField]
    private string m_VolumeKeyDataPath;

    [Tooltip("If true, then the surface data to access only needs to 'begin with' SurfaceKeyDataPath; this is useful" +
        "when a timestep ID is appended to the path for timevarying data.  If false, the SurfaceKeyDataPath must " +
        "be an exact match with one of the DataImpressions in the scene."), SerializeField]
    private bool m_KeyDataPathCanIncludeSuffix = true;

    [Tooltip("Value returned by GetValueAtWorldPoint() when data are not available/loaded and when requesting data " +
        "at a point that is located outside of the volume."), SerializeField]
    private float m_OutOfRangeDataValue;


    // Runtime-only Private Variables
    private SimpleVolumeDataImpression m_CurrentVolumeImpression;
    private DataImpressionGroup m_CurrentDataImpressionGroup;
    private RawDataset m_CurrentRawDataset;
    private BitArray m_PerVoxelVisibility;
}
