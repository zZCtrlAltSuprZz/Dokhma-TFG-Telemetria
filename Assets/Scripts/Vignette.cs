using UnityEngine;

public class Vignette : MonoBehaviour
{
    public float intensity = 0.4f;
    public float softness = 0.5f;
    public Color color = Color.black;

    private Material mat;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/Vignette"));
        }
        mat.SetFloat("_Intensity", intensity);
        mat.SetFloat("_Softness", softness);
        mat.SetColor("_Color", color);
        Graphics.Blit(src, dest, mat);
    }
}