using UnityEngine;
using System.Collections;

public class EchoRevealable : MonoBehaviour
{
    public Color revealColor = Color.cyan;
    public const float revealDuration = 1.5f;

    private Renderer rend;
    private Material mat;
    private Coroutine revealCoroutine;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material; // создаёт инстанс материала для этого объекта
        mat.DisableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.black);
    }

    public void Reveal()
    {
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);

        revealCoroutine = StartCoroutine(RevealFade());
    }

    private IEnumerator RevealFade()
    {
        mat.EnableKeyword("_EMISSION");
        float t = 0f;

        mat.SetColor("_EmissionColor", revealColor);

        while (t < revealDuration)
        {
            t += Time.deltaTime;
            float fade = Mathf.Lerp(1f, 0f, t / revealDuration);
            mat.SetColor("_EmissionColor", revealColor * fade);
            yield return null;
        }

        mat.SetColor("_EmissionColor", Color.black);
        mat.DisableKeyword("_EMISSION");
    }
}