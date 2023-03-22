/* CSci-5609 Support Code created by Prof. Dan Keefe, Fall 2023 */

using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// This class turns the GameObject it is attached to into a "data probe".  When the GameObject is placed
/// inside a data volume, this class looks up the raw voxel data value at the origin of the GameObject.  If the
/// GameObject is moved, the data value is updated.  The current data value "under" the probe can be accessed via
/// public properties.  The class provides several UnityEvents that you can subscribe to if you want to receive
/// a callback when the probe's readings change.  These are especially useful for visual programming in the Unity
/// Editor.  Click the + sign under the OnDataValueChanged section in the Editor GUI, then drag a GameObject into the
/// section that is created and select the method on that object that you would like to call when the probe is moved
/// and the data value "under" it changes.
///
/// The class provides some simple mouse-and-keyboard interactive controls for moving the probe relative to a
/// nearby surface (e.g., the ground, an airplane wing), and an ABRSurfaceDataAccessor must be provided for this
/// surface.  Move the mouse over the surface to snap the probe to a location on the surface.  Then, to move the
/// probe up/down relative to the surface, simply hold down the up/down arrow keys.
/// </summary>
public class VolumeDataProbe : MonoBehaviour
{

    /// <summary>
    /// Returns true if the probe is currently located inside the volume data.
    /// </summary>
    public bool IsInsideDataVolume {
        get {
            return m_InsideDataVolume;
        }
    }

    /// <summary>
    /// If IsInsideDataVolume is true, then this is the current data value under the probe.  Otherwise, it stores the
    /// value as of the last time the probe could accurately read data.
    /// </summary>
    public float DataValueUnderProbe {
        get {
            return m_DataValueUnderProbe;
        }
    }

    /// <summary>
    /// If IsInsideDataVolume is true, then this is the current normalized data value under the probe.  Otherwise,
    /// it stores the value as of the last time the probe could accurately read data.
    /// </summary>
    public float NormalizedDataValueUnderProbe {
        get {
            return m_NormalizedDataValueUnderProbe;
        }
    }

    [Header("Volume Data Source")]

    [Tooltip("Drag an ABRVolumeDataAccessor here to tell the probe which volume dataset to use.")]
    public ABRVolumeDataAccessor m_VolumeDataAccessor;


    [Header("Interactive Controls")]

    [Tooltip("To move the probe with the mouse so that it lies on or above a surface, drag an " +
        "ABRSurfaceDataAccessor here to tell the probe which surface data to use.")]
    public ABRSurfaceDataAccessor m_SurfaceDataAccessor;

    [Tooltip("When moving the probe relative to a surface, the probe is positioned at this consistent " +
        "height above the surface.")]
    [Range(0, 500)]
    public float heightAboveSurface;

    [Tooltip("Press and hold this key to move the probe up (+Y).")]
    public KeyCode raiseHeightKey;
    [Tooltip("Press and hold this key to move the probe down (-Y).")]
    public KeyCode lowerHeightKey;
    [Tooltip("Increase to make the height adjustment go faster.")]
    public float heightAdjustmentSensativity;


    [Header("Callback Functions")]

    [Tooltip("Add a listener to this event to do something whenever the data probe enters the volume from somewhere " +
        "outside of the volume.")]
    public UnityEvent OnEnterVolume;

    [Tooltip("Add a listener to this event to do something whenever the data probe leaves the volume.")]
    public UnityEvent OnExitVolume;


    [Tooltip("Add a listener to this event to do something whenever the data probe moves to a position where the " +
        "data value is different than the previous data value.  The float parameter for this event reports the " +
        "data value at the new location.")]
    public UnityEvent<float> OnDataValueChanged;

    [Tooltip("Add a listener to this event to do something whenever the data probe moves to a position where the " +
        "data value is different than the previous data value.  The float parameter for this event reports the " +
        "normalized data value at the new location.")]
    public UnityEvent<float> OnNormalizedDataValueChanged;


    private void Reset()
    {
        m_VolumeDataAccessor = null;
        m_SurfaceDataAccessor = null;
        heightAboveSurface = 1;
        raiseHeightKey = KeyCode.UpArrow;
        lowerHeightKey = KeyCode.DownArrow;
        heightAdjustmentSensativity = 0.1f;
        OnEnterVolume = null;
        OnExitVolume = null;
        OnDataValueChanged = null;
        OnNormalizedDataValueChanged = null;
    }


    // Start is called before the first frame update
    void Start()
    {
        // if not already set in the editor, look for a volume data accessor attached to the same gameobject
        if (m_VolumeDataAccessor == null) {
            m_VolumeDataAccessor = GetComponent<ABRVolumeDataAccessor>();
        }
        if (m_VolumeDataAccessor == null) {
            Debug.LogWarning("Missing an ABRVolumeDataAccessor");
        }
        m_InsideDataVolume = false;
    }


    // Called once each frame
    private void Update()
    {
        if (Input.GetKey(raiseHeightKey)) {
            heightAboveSurface += heightAdjustmentSensativity;
        }
        if (Input.GetKey(lowerHeightKey)) {
            heightAboveSurface -= heightAdjustmentSensativity;
        }
        if ((m_SurfaceDataAccessor != null) && (m_SurfaceDataAccessor.GetGameObject() != null)) {
            MeshCollider mc = m_SurfaceDataAccessor.GetMeshCollider();
            if (mc == null) {
                mc = m_SurfaceDataAccessor.GetGameObject().AddComponent<MeshCollider>();
            }
            if (mc != null) {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (mc.Raycast(r, out hitInfo, Camera.main.farClipPlane)) {
                    Vector3 newProbeWorldPos = hitInfo.point + heightAboveSurface * Vector3.up;
                    transform.position = newProbeWorldPos;
                }
            }
        }

        if (m_VolumeDataAccessor != null) {
            // Get the world space (x,y,z) position of the GameObject this script is attached to.
            Vector3 probePosInWorldSpace = this.gameObject.transform.position;

            // The GameObject could be moved anywhere in the scene, we only want to lookup data values if the current
            // position is within the bounds of the volume.  Start by checking to see if we are in the volume.
            bool nowInsideData = m_VolumeDataAccessor.ContainsWorldSpacePoint(probePosInWorldSpace);
            if (nowInsideData && !m_InsideDataVolume) {
                m_InsideDataVolume = true;
                OnEnterVolume.Invoke();
                //Debug.Log("Enter volume");
            } else if (!nowInsideData && m_InsideDataVolume) {
                m_InsideDataVolume = false;
                OnExitVolume.Invoke();
                //Debug.Log("Exit volume");
            }

            if (m_InsideDataVolume) {
                float dataVal = m_VolumeDataAccessor.GetValueAtWorldSpacePoint(probePosInWorldSpace);
                if (dataVal != m_DataValueUnderProbe) {
                    m_DataValueUnderProbe = dataVal;
                    OnDataValueChanged.Invoke(m_DataValueUnderProbe);

                    m_NormalizedDataValueUnderProbe = m_VolumeDataAccessor.NormalizeDataValue(dataVal);
                    OnNormalizedDataValueChanged.Invoke(m_NormalizedDataValueUnderProbe);

                    //Debug.Log("Data: " + m_DataValueUnderProbe + "     " + m_NormalizedDataValueUnderProbe);
                }
            }
        }
    }
    

    // Runtime only private vars
    private bool m_InsideDataVolume;
    private float m_DataValueUnderProbe;
    private float m_NormalizedDataValueUnderProbe;
}
