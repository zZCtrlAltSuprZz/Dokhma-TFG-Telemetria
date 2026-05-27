using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DynamicDoF : MonoBehaviour
{
    public Transform target;
    public PostProcessVolume volume;
    public float smoothSpeed = 5f;

    private DepthOfField dof;

    void Start()
    {
        volume.profile.TryGetSettings(out dof);
    }

    void Update()
    {
        if (dof == null || target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        dof.focusDistance.value = Mathf.Lerp(
            dof.focusDistance.value,
            dist,
            Time.deltaTime * smoothSpeed
        );
    }
}