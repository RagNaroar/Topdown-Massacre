using UnityEngine;

public class Effect : MonoBehaviour
{
    public float Lifetime = 0.2f;

    // Update is called once per frame
    void Start()
    {
        Destroy(gameObject, Lifetime);
    }
}
