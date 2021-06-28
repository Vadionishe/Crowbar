using UnityEngine;

namespace Crowbar
{
    public class RandomGroundInEditor : MonoBehaviour
    {
        public Vector3 maxScale;
        public Vector3 minScale;

        [ContextMenu("RandomTransform")]
        public void RandomTransform()
        {
            RandomScale();
            RandomRotate();
        }

        public void RandomScale()
        {
            transform.localScale = new Vector3(Random.Range(minScale.x, maxScale.x), Random.Range(minScale.y, maxScale.y), Random.Range(minScale.y, maxScale.y));
        }

        public void RandomRotate()
        {
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
        }
    }
}
