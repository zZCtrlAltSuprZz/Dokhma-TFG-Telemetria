using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraShake : MonoBehaviour
{
    [SerializeField] private float defaultDuration = 0.08f;
    [SerializeField] private float defaultAmplitude = 0.6f;
    [SerializeField] private float defaultFrequency = 20f;

    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultAmplitude, defaultFrequency);
    }

    public void Shake(float duration, float amplitude, float frequency)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, amplitude, frequency));
    }

    private IEnumerator ShakeRoutine(float duration, float amplitude, float frequency)
    {
        noise.AmplitudeGain = amplitude;
        noise.FrequencyGain = frequency;

        yield return new WaitForSeconds(duration);

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;

        shakeRoutine = null;
    }
}