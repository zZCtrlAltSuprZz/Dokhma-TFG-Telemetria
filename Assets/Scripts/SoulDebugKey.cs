using UnityEngine;

public class SoulDebugKey : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (SoulManager.Instance != null)
                SoulManager.Instance.AddSouls(100);
        }
    }
}