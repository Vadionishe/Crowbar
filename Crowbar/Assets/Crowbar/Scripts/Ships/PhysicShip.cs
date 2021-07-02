using Mirror;
using System.Collections;
using UnityEngine;

namespace Crowbar.Ship
{
    public class PhysicShip : MonoBehaviour
    {
        public UnderwaterShip underwaterShip;
        public CheckWaterShip waterChecker;

        public float forceCollision = 200f;
        public float speedForCrash = 5f;
        public float maxSpeed = 5f;
        public float speedUp = 0.2f;
        public float gravity = 10f;
        public float gravityWater = 0.1f;
        public float electricDown = 0.01f;
        public float fuelDown = 0.05f;

        public bool canMove = true;
        public string layerGround = "GroundCollision";

        public ElectricStorage electricStorage;
        public FuelStorage fuelStorage;

        private Vector2 lastSpeed;
        private Vector2 directionMove;
        private Rigidbody2D m_rigidBody;

        public void Move(Vector2 direction)
        {
            directionMove = direction;      
        }

        private IEnumerator CollisionGround()
        {
            canMove = false;

            yield return new WaitForSeconds(0.6f);

            canMove = true;
        }

        private void Start()
        {
            m_rigidBody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!NetworkServer.active)
                return;

            m_rigidBody.gravityScale = (waterChecker.CheckCollision()) ? gravityWater : gravity;

            if (electricStorage.electric >= electricDown && fuelStorage.fuel >= fuelDown && canMove)
            {
                underwaterShip.SetMotorStateServer(MotorRotate.Side.Left, directionMove.x > 0);
                underwaterShip.SetMotorStateServer(MotorRotate.Side.Right, directionMove.x < 0);
                underwaterShip.SetMotorStateServer(MotorRotate.Side.Down, directionMove.y > 0);
                underwaterShip.SetMotorStateServer(MotorRotate.Side.Up, directionMove.y < 0);

                if (m_rigidBody.velocity.magnitude < maxSpeed)
                {
                    electricStorage.ChangeElectric(-electricDown);
                    fuelStorage.ChangeFuel(-fuelDown);
                    m_rigidBody.AddForce(directionMove * speedUp);
                }
            }

            lastSpeed = new Vector2(Mathf.Abs(m_rigidBody.velocity.x), Mathf.Abs(m_rigidBody.velocity.y));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == layerGround && canMove)
            {
                if (lastSpeed.x > speedForCrash || lastSpeed.y > speedForCrash) 
                {
                    m_rigidBody.AddForce((transform.position - collision.transform.position).normalized * forceCollision);
                    underwaterShip.AddCrack();

                    StartCoroutine(CollisionGround());
                }
            }
        }
    }
}

