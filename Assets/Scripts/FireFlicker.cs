using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    Light fireLight;
    public float minIntensity = 1.5f;
    public float maxIntensity = 3.5f;
    public float flickerSpeed = 8f;

    void Start() => fireLight = GetComponent<Light>();

    void Update()
    {
        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity,
            Mathf.PerlinNoise(Time.time * flickerSpeed, 0));
    }
}