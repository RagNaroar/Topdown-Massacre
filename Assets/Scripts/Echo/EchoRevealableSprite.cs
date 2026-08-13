using UnityEngine;
using System.Collections;

public class EchoRevealableSprite : MonoBehaviour
{
    public Color hiddenColor = new Color(0.1f, 0f, 0f, 1f);
    public Color revealedColor = Color.red;
    public float revealDuration = 1.5f;

    private SpriteRenderer sr;
    private Coroutine revealCoroutine;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = hiddenColor;
    }

    public void Reveal()
    {
        if (revealCoroutine != null)
            StopCoroutine(revealCoroutine);

        revealCoroutine = StartCoroutine(RevealFade());
    }

    private IEnumerator RevealFade()
    {
        sr.color = revealedColor;
        float t = 0f;

        while (t < revealDuration)
        {
            t += Time.deltaTime;
            sr.color = Color.Lerp(revealedColor, hiddenColor, t / revealDuration);
            yield return null;
        }

        sr.color = hiddenColor;
    }
}