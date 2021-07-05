using UnityEngine;

namespace Crowbar
{
    public class PickManager : MonoBehaviour
    {
        public static IPickInfo pickInfo;
        public static GameObject pickObject;

        private void Update()
        {          
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector3.forward);

            if (hits.Length > 0)
            {
                float distance = float.MaxValue;
                RaycastHit2D nearestHit = hits[0];

                foreach (RaycastHit2D hit in hits)
                {
                    float _distance = Vector2.Distance(hit.transform.position, mousePosition);

                    if (_distance < distance)
                    {
                        distance = _distance;
                        nearestHit = hit;
                    }
                }

                if (nearestHit.transform != null)
                {
                    IPickInfo info = nearestHit.transform.GetComponent<IPickInfo>();

                    if (info != null)
                    {
                        if (pickInfo != info)
                        {
                            Unpick();

                            pickObject = nearestHit.transform.gameObject;
                            pickInfo = info;

                            if (pickObject != null)
                                pickInfo.Pick();
                        }
                    }
                    else
                    {
                        Unpick();
                    }
                }
                else
                {
                    Unpick();
                }  
            }
            else
            {
                Unpick();
            }
        }

        private void Unpick()
        {
            if (pickInfo != null)
                pickInfo.UnPick();

            if (pickObject != null)
                pickObject = null;

            pickInfo = null;
        }
    }
}
