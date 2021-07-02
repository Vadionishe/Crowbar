using UnityEngine;

namespace Crowbar
{
    [ExecuteInEditMode]
    public class SpawnerObjects : MonoBehaviour
    {
        public GameObject prefabObject;
        public GameObject parent;

        public Vector2 leftUpPoint;
        public Vector2 rightDownPoint;

        public int count;

        [ContextMenu("Spawn")]
        public void Spawn()
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 position = new Vector3(Random.Range(leftUpPoint.x, rightDownPoint.x), Random.Range(leftUpPoint.y, rightDownPoint.y), parent.transform.position.z);
                GameObject _object = Instantiate(prefabObject, position, Quaternion.identity, parent.transform);

                _object.GetComponent<DownObject>().PlaceObject();
            }
        }
    }
}
