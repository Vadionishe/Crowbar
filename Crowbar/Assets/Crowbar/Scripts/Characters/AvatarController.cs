using UnityEngine;
using Mirror;

namespace Crowbar
{
    /// <summary>
    /// Avatar controller for character
    /// </summary>
    public class AvatarController : MonoBehaviour
    {
        #region Variables
        [Header("Avatar properties")]
        public Transform target;
        public GameObject leftHand;
        public GameObject rightHand;
        public GameObject glass;
        public GameObject mouth;
        public GameObject hat;
        public TextMesh textName;
        public bool isLocal;      
        public bool canFlip;

        public int idHat;

        private float movementSmoothing;
        private Vector3 m_velocity;
        #endregion

        #region Functions   
        public void SetName(string name)
        {
            textName.text = name;
        }

        public void SetSkin(int idHat)
        {
            this.idHat = idHat;

            SkinsManager.SetHat(hat.GetComponent<SpriteRenderer>(), idHat);
        }

        private void SetFlipping(bool isRight)
        {
            if (canFlip)
            {
                transform.localScale = new Vector3((isRight) ? 1f : -1f, 1f, 1f);               

                leftHand.SetActive(isRight);
                rightHand.SetActive(!isRight);
            }
        }

        private void Start()
        {
            canFlip = true;
        }

        private void Update()
        {           
            movementSmoothing = Time.deltaTime * 2;
            textName.transform.localScale = new Vector3(transform.localScale.x, 1f, 1f);

            if (target != null)
            {
                Vector3 newPos = Vector3.SmoothDamp(transform.position, target.position, ref m_velocity, movementSmoothing);

                newPos.z = target.position.z;
                transform.position = newPos;
                transform.rotation = target.rotation;
                transform.SetParent(target.parent);
            }

            if (isLocal)
            {
                var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                SetFlipping(mousePosition.x >= transform.position.x);
            }
        }
        #endregion
    }
}
