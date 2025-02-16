using System.Collections;
using UnityEngine;

public class Bob : MonoBehaviour
{
    public float cycleDuration;

    // The two target y positions for the bobbing motion.
    private float targetUpY;
    private float targetDownY;
    
    // Height of the object (in world space) and the offset from the pivot to the bottom of the bounds.
    private float height;
    private float pivotToBottomOffset;
    
    // Phase offset so that the bobbing starts at the object's current position.
    private float phaseOffset;

    void Start()
    {
        // Get the renderer component to compute the bounds.
        Transform obj = transform.Find("Object");
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("BobbingObject requires a Renderer on the same GameObject.");
            enabled = false;
            return;
        }
        
        // Compute the bounds and object height.
        Bounds bounds = rend.bounds;
        height = bounds.size.y;
        
        // The bottom of the object (in world space)
        float bottomY = bounds.min.y;
        // Calculate the offset from the pivot (transform.position.y) to the bottom of the object.
        pivotToBottomOffset = transform.position.y - bottomY;
        
        // Define our desired waterline ratios.
        // For 80% visible: waterline should be at 20% of the object's height from the bottom.
        float upRatio = 0.2f;
        // For 10% visible: waterline should be at 90% of the object's height from the bottom.
        float downRatio = 0.8f;
        
        // Calculate target positions for the pivot using the formula:
        // transform.position.y = pivotToBottomOffset - (ratio * height)
        targetUpY = pivotToBottomOffset - upRatio * height;
        targetDownY = pivotToBottomOffset - downRatio * height;
        
        // Compute the current interpolation fraction (phase) based on the current y position.
        // The idea is that our bobbing will interpolate between targetDownY (at phase=0) and targetUpY (at phase=1).
        float initialPhase = Mathf.InverseLerp(targetDownY, targetUpY, transform.position.y);
        // We want to use a sine function that returns (sin(θ)+1)/2. So solve:
        // (sin(phaseOffset) + 1)/2 = initialPhase   ->   phaseOffset = Asin(2*initialPhase - 1)
        phaseOffset = Mathf.Asin(2 * initialPhase - 1);
    }

    void Update()
    {
        // Determine the angular speed so that one full cycle takes cycleDuration seconds.
        float angularSpeed = 2 * Mathf.PI / cycleDuration;
        
        // Calculate the current phase using a sine wave.
        // Mathf.Sin returns values between -1 and 1; mapping to 0-1:
        float phase = (Mathf.Sin(Time.time * angularSpeed + phaseOffset) + 1f) / 2f;
        
        // Interpolate between the down and up target positions.
        float newY = Mathf.Lerp(targetDownY, targetUpY, phase);
        
        // Update the object's position while keeping x and z unchanged.
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
