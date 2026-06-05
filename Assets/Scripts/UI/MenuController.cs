using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private CameraTransition cameraTransition;

    private void Start()
    {
        MusicManager.Instance.PlayMainMenuMusic();
    }

    public void OnPlayButtonClicked()
    {

        cameraTransition.StartTransition();
    }
}