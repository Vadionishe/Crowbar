using UnityEngine;

namespace Crowbar.System
{
	public class Parallax : MonoBehaviour
	{
        public GameObject target;
        public float xParallax = 0.8f;
        public float yParallax = 1;

        private Vector3 startOffset;

        private void Awake()
        {
            startOffset = transform.position - target.transform.position;
        }

        private void Update()
        {
            Vector3 position = new Vector3(target.transform.position.x * xParallax + startOffset.x, 
                target.transform.position.y * yParallax + startOffset.y, 
                transform.position.z);

            transform.position = position;
        }
    }
}
