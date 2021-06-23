namespace Crowbar
{
    using UnityEngine;

    public class HandController : MonoBehaviour
    {
        public float angleHandAttack;
        public float speedHandAttack;
        public float zPos = 0.1f;
        public bool localHand;
        public bool isAttack;
        public bool isRight;
        public Vector3 mousePosOther;
        public Transform grabPosition;

        public void HandAttackOtherPlayer()
        {
            if (isAttack)
            {
                float angle = Vector2.Angle(Vector2.right, mousePosOther - transform.position);
                Vector3 newPos = transform.localPosition;

                angle = (transform.position.y < mousePosOther.y) ? angle : -angle;
                angle = (transform.position.x >= mousePosOther.x) ? angle + 180 : angle;
                newPos.z = (transform.position.x <= mousePosOther.x) ? -zPos : zPos;

                transform.eulerAngles = new Vector3(0f, 0f, angle);
                transform.localPosition = newPos;

                float newAngle = Mathf.PingPong(Time.time * speedHandAttack, angleHandAttack);
                float offset = (transform.position.x >= mousePosOther.x) ? -angleHandAttack : 0;

                transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z + offset + newAngle);
            }
        }

        private void Update()
        {
            if (localHand)
            {
                MoveHandToMouse();

                if (Input.GetKey(KeyCode.Mouse0))
                {
                    HandAttackAnimation();
                    isAttack = true;
                }
                else
                {
                    isAttack = false;
                }
            }
        }

        private void MoveHandToMouse()
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = transform.localPosition;
            float angle = Vector2.Angle(Vector2.right, mousePosition - transform.position);         

            angle = (transform.position.y < mousePosition.y) ? angle : -angle;
            angle = (transform.position.x >= mousePosition.x) ? angle + 180 : angle;
            newPos.z = (transform.position.x <= mousePosition.x) ? -zPos : zPos;

            transform.eulerAngles = new Vector3(0f, 0f, angle);
            transform.localPosition = newPos;
        }

        private void HandAttackAnimation()
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float angle = Mathf.PingPong(Time.time * speedHandAttack, angleHandAttack);
            float offset = (transform.position.x >= mousePosition.x) ? -angleHandAttack : 0; 

            transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z + offset + angle);
        }
    }
}
