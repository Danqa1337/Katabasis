using System.Threading.Tasks;
using UnityEngine;

public class PutBackToPoolOnAnimationEnds : MonoBehaviour
{
    public void OnEnable()
    {

        wait();
    }

    public async Task wait()
    {
        await Task.Delay(GetComponent<Animator>().GetCurrentAnimatorClipInfo(0).Length * 1000);
        Pooler.Put(gameObject);
    }
}
