using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Position Recoil")]
    [SerializeField] private Vector3 recoilPosition;
    [SerializeField] private float returnSpeed = 8f;
    [SerializeField] private float snappiness = 18f;

    [Header("Rotation Recoil")]
    [SerializeField] private Vector3 recoilRotation;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    private Vector3 currentPosition;
    private Vector3 targetPosition;

    private void Update()
    {
        targetPosition = Vector3.Lerp(
            targetPosition,
            Vector3.zero,
            returnSpeed * Time.deltaTime
        );

        currentPosition = Vector3.Lerp(
            currentPosition,
            targetPosition,
            snappiness * Time.deltaTime
        );

        targetRotation = Vector3.Lerp(
            targetRotation,
            Vector3.zero,
            returnSpeed * Time.deltaTime
        );

        currentRotation = Vector3.Lerp(
            currentRotation,
            targetRotation,
            snappiness * Time.deltaTime
        );

        transform.localPosition = currentPosition;

        transform.localRotation =
            Quaternion.Euler(currentRotation);
    }

    public void PlayRecoil()
    {
        targetPosition += recoilPosition;
        targetRotation += recoilRotation;
    }
}