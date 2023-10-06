using UnityEngine;

public class DestroyWithDelay : MonoBehaviour
{
    public float delay = 0.1f;
    private void Awake()
    {
        Destroy(gameObject, delay);
    }
}
