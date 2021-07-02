using UnityEngine;

namespace Crowbar
{
    public class RandomAnimationGrass : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Animator>().speed = Random.Range(0.2f, 1f);
        }
    }
}
