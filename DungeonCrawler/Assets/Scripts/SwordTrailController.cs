using UnityEngine;

public class SwordTrailController : MonoBehaviour
{
    public TrailRenderer trail;

    private void Awake()
    {
        if(trail == null)
        {
            trail = GetComponent<TrailRenderer>();
        }

        if(trail != null)
        {
            trail.emitting = false;
        }
    }

    public void EnableTrail()
    {
        if(trail != null)
        {
            trail.emitting = true;
        }
    }

    public void DisableTrail()
    {
        if (trail != null)
        {
            trail.emitting = false;
        }
    }
}
