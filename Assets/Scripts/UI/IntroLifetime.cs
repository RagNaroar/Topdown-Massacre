using UnityEngine;
using UnityEngine.UI;
public class IntroLifetime : MonoBehaviour
{
    public float Lifetime = 7f;
    private void Start()
    {
      Destroy(gameObject, Lifetime);
    }
}
