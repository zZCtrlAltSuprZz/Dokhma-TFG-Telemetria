using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private CameraTransition cameraTransition;
    public void OnPlayButtonClicked()
    {

        cameraTransition.StartTransition();
    }
}