using UnityEngine;

namespace Crowbar
{
    [ExecuteInEditMode]
    public class DownObject : MonoBehaviour
    {
        [ContextMenu("PlaceObject")]
        public void PlaceObject()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, LayerMask.GetMask("GroundCollision"));

            if (new Vector2(hit.point.x, hit.point.y) == new Vector2(transform.position.x, transform.position.y))
            {
                DestroyImmediate(gameObject);

                return;
            }

            if (hit.collider != null)
            {
                transform.position = hit.point;
                transform.rotation = Quaternion.FromToRotation(Vector2.up, hit.normal);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}
