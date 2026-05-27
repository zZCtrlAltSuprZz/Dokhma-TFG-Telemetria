using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CameraTransition : MonoBehaviour
{
    [Header("Destino de la transición")]
    [SerializeField] private Transform target;

    [Header("Configuración")]
    [SerializeField] private float duration = 2.0f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Movimiento idle")]
    [SerializeField] private float driftSpeed = 0.3f;
    [SerializeField] private float driftAmount = 0.5f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool transitioning = false;
    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        StartCoroutine(DriftCamera());
    }

    private IEnumerator DriftCamera()
    {
        float t = 0f;

        while (!transitioning)
        {
            t += Time.deltaTime * driftSpeed;

            // Movimiento suave en X e Y usando senos con frecuencias distintas
            float offsetX = Mathf.Sin(t) * driftAmount;
            float offsetY = Mathf.Sin(t * 0.7f) * driftAmount * 0.5f;

            transform.position = initialPosition + new Vector3(offsetX, offsetY, 0f);

            // Rotación suave mirando ligeramente en distintas direcciones
            float rotY = Mathf.Sin(t * 0.5f) * 1.5f;
            transform.rotation = initialRotation * Quaternion.Euler(0f, rotY, 0f);

            yield return null;
        }

        // Vuelve suavemente a la posición inicial antes de la transición
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    public void StartTransition()
    {
        if (!transitioning)
            StartCoroutine(DoTransition());
    }

    private IEnumerator DoTransition()
    {
        transitioning = true;

        // Fade out del canvas
        yield return StartCoroutine(FadeCanvas(1f, 0f));

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = easeCurve.Evaluate(Mathf.Clamp01(elapsed / duration));

            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        //SceneManager.LoadScene("Blocking");
        canvasGroup.gameObject.SetActive(false);

        SceneManager.LoadScene("Blocking", LoadSceneMode.Single);
    }

    private IEnumerator FadeCanvas(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}