/* CSci-5609 Support Code created by Prof. Dan Keefe, Fall 2023 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// This class helps with managing the display and animation of time-varying data.  Animation can be done automatically
/// with speed set by the framesPerSecond parameter or paused and controlled manually by calling AdvanceOneTimestep()
/// and RewindOneTimestep().  
/// </summary>
public class TimestepAnimationController : MonoBehaviour
{
    [Header("Timestep Setup")]

    [Tooltip("The number of the first timestep in the sequence.")]
    public int firstTimestepNumber = 4;

    [Tooltip("The number of the last timestep in the sequence.")]
    public int lastTimestepNumber = 74;

    [Tooltip("Since the data are large, visualizations do not always display every timestep.  This is the amount to " +
        "increment the timestep number when iterating between first and last.  Set it to 1 to display every timestep, " +
        "2 to display every other timestep, etc.")]
    public int timestepNumberInc = 14;


    [Header("Animation Controls")]

    [Tooltip("Paused only impacts automated playback.  Calling AdvanceOneFrame() or RewindOneFrame() will update the " +
        "display even when paused is true.")]
    public bool paused = false;

    [Tooltip("The animation will attempt to advance to the next timestep to match the framerate specified; however, " +
        "if the data are large, it may not always be able to achieve this.")]
    [Range(0.1f, 60.0f)]
    public float framesPerSecond = 1.0f;


    [Header("Callback Function")]

    [Tooltip("Drag a GameObject here and select a function on it to call whenever the timestep is updated.")]
    public UnityEvent<int> OnTimestepChanged;


    // private runtime-only vars
    List<int> timestepNumbers;
    // note: this is the index into the timestepNumbers and timestepKeyData arrays, NOT the current timestep number.
    int currentTimestepIndex;
    float lastFrameTime;


    /// <summary>
    /// Returns true if the animation will display the specified timestep number. 
    /// </summary>
    public bool HasTimestep(int timestepNumber)
    {
        return timestepNumbers.Contains(timestepNumber);
    }

    /// <summary>
    /// Returns the number of the timestep that is currently displayed.
    /// </summary>
    public int CurrentTimestep()
    {
        if (timestepNumbers.Count > 0) {
            return timestepNumbers[currentTimestepIndex];
        } else {
            return -1;
        }
    }

    /// <summary>
    /// Moves to the next timstep in the squence.  If timestepNumberInc is not 1, this could skip some timesteps.
    /// If there are no more timesteps in the sequence, this will loop back to the first is the sequence.
    /// </summary>
    public void AdvanceOneTimestep()
    {
        currentTimestepIndex++;
        if (currentTimestepIndex >= timestepNumbers.Count) {
            currentTimestepIndex = 0;
        }
        NotifyListeners();
    }

    /// <summary>
    /// Moves back one timestep in the sequence.  If timestepNumberInc is not 1, this could skip some timesteps.
    /// If this rewinds past the first timestep in the sequence, it will loop to the last available timestep.
    /// </summary>
    public void RewindOneTimestep()
    {
        currentTimestepIndex--;
        if (currentTimestepIndex < 0) {
            currentTimestepIndex = timestepNumbers.Count - 1;
        }
        NotifyListeners();
    }


    /// <summary>
    /// Updates to display the specified timestep.
    /// </summary>
    public void SetTimestep(int timestepNumber)
    {
        int closestIndex = timestepNumbers.IndexOf(timestepNumber);
        if ((closestIndex == -1) && (timestepNumbers.Count > 0)) {
            int closestDiff = Mathf.Abs(timestepNumber - timestepNumbers[0]);
            for (int i = 1; i < timestepNumbers.Count; i++) {
                int diff = Mathf.Abs(timestepNumber - timestepNumbers[i]);
                if (diff < closestDiff) {
                    closestIndex = i;
                    closestDiff = diff;
                }
            }
        }

        if ((closestIndex != -1) && (closestIndex != currentTimestepIndex)) {
            currentTimestepIndex = closestIndex;
            NotifyListeners();
        }
    }



    private void NotifyListeners()
    {
        OnTimestepChanged.Invoke(timestepNumbers[currentTimestepIndex]);
        Debug.Log("Current Timestep = " + timestepNumbers[currentTimestepIndex]);
    }

    private void Start()
    {
        currentTimestepIndex = 0;
        timestepNumbers = new List<int>();
        for (int i = firstTimestepNumber; i <= lastTimestepNumber; i += timestepNumberInc) {
            timestepNumbers.Add(i);
        }
        lastFrameTime = Time.time - 1.0f / framesPerSecond;
    }

    void Update()
    {
        /*
        if ((!paused) && (Time.time - lastFrameTime > 1.0f / framesPerSecond)) {
            AdvanceOneTimestep();
            lastFrameTime = Time.time;
        }
        */
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AdvanceOneTimestep();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RewindOneTimestep();
        }
    }
}
