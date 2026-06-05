using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class DoorUnlock : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private int cost = 500;
    [SerializeField] private float openHeight = 5f;
    [SerializeField] private float openDuration = 1.5f;
    [SerializeField] private bool destroyAfterOpen = true;

    [Header("References")]
    [SerializeField] private Transform doorVisual;
    [SerializeField] private Collider doorCollider;

    [Header("Interaction UI")]
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private GameObject interactionPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;

    [SerializeField] private NavMeshObstacle navMeshObstacle;

    private bool playerInside;
    private bool isOpened;
    private Vector3 closedPosition;
    private Vector3 openedPosition;

    private void Awake()
    {
        if (doorVisual == null)
            doorVisual = transform;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (navMeshObstacle == null)
            navMeshObstacle = GetComponent<NavMeshObstacle>();

        if (doorCollider == null)
            doorCollider = GetComponent<Collider>();

        closedPosition = doorVisual.position;
        openedPosition = closedPosition + Vector3.up * openHeight;

        HideText();
    }

    private void Update()
    {
        if (!playerInside || isOpened)
            return;

        ShowText();

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E pulsada dentro de la puerta");
            TryOpenDoor();
        }

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            Debug.Log("Cuadrado pulsado dentro de la puerta");
            TryOpenDoor();
        }
    }

    private void PlayOpenSound()
    {
        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);
    }

    private void TryOpenDoor()
    {
        Debug.Log("Intentando abrir puerta");

        if (SoulManager.Instance == null)
        {
            Debug.LogError("No existe SoulManager.Instance");
            return;
        }

        if (!SoulManager.Instance.TrySpendSouls(cost))
        {
            if (interactionText != null)
                interactionText.text = "Interact to open (" + cost + ")";

            return;
        }
        PlayOpenSound();
        StartCoroutine(OpenDoorRoutine());
    }

    private IEnumerator OpenDoorRoutine()
    {
        isOpened = true;
        HideText();

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (navMeshObstacle != null)
            navMeshObstacle.enabled = false;

        float timer = 0f;

        while (timer < openDuration)
        {
            timer += Time.deltaTime;
            float t = timer / openDuration;

            doorVisual.position = Vector3.Lerp(closedPosition, openedPosition, t);

            yield return null;
        }

        doorVisual.position = openedPosition;

        if (destroyAfterOpen)
        {
            yield return new WaitForSeconds(0.5f);

            if (doorVisual != null)
                Destroy(doorVisual.gameObject);
        }
    }

    private void ShowText()
    {
        if (interactionText != null)
        {

            interactionText.gameObject.SetActive(true);
            interactionText.text = "Interact to open (" + cost + ")";
        }
        if (interactionPanel != null)
            interactionPanel.SetActive(true);
    }

    private void HideText()
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jugador dentro del trigger de puerta");
            playerInside = true;
            ShowText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            HideText();
        }
    }
}