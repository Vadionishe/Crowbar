namespace Crowbar.Ship
{
    using System.Collections;
    using UnityEngine;
    using Mirror;

    public class ShipGun : NetworkBehaviour
    {
        public float cooldown = 1.5f;
        public float speedRotateGun = 1f;
        public float speedRotateSmooth = 0.1f;
        public float speedShot = 0.2f;
        public float electricDown = 0.2f;

        public Transform spawnBulletPosition;
        public ShipBullet bulletPrefab;
        public ControllerShipGun controllerGun;
        public ElectricStorage electricStorage;
        public ParticleSystem particleShot;

        private bool canShot = true;

        private Vector3 m_targetRotation;
        private Vector3 m_velocityRotate;

        [ClientRpc]
        public void RpcShot()
        {
            particleShot.Play();
        }

        [Server]
        [ContextMenu("Shot")]
        public void Shot()
        {
            if (canShot && controllerGun.bullet > 0 && electricStorage.electric >= electricDown)
            {
                electricStorage.ChangeElectric(-electricDown);

                StartCoroutine(Cooldown());
                controllerGun.ChangeBullet(-1);

                Vector3 position = spawnBulletPosition.transform.position + new Vector3(0, 0, 0.0001f);
                ShipBullet bullet = Instantiate(bulletPrefab, position, Quaternion.identity, null);

                NetworkServer.Spawn(bullet.gameObject);
                bullet.Push(spawnBulletPosition.transform.right * speedShot);

                RpcShot();
            }
        }

        [Server]
        public void Rotate(float angle)
        {
            transform.Rotate(new Vector3(0, 0, speedRotateGun * angle));

            RpcSetTargetAngle(transform.localEulerAngles);
        }

        [ClientRpc]
        public void RpcSetTargetAngle(Vector3 angle)
        {
            m_targetRotation = angle;
        }

        private void Update()
        {
            if (!isServer)
            {
                float angleX = Mathf.SmoothDampAngle(transform.localEulerAngles.x, m_targetRotation.x, ref m_velocityRotate.x, speedRotateSmooth);
                float angleY = Mathf.SmoothDampAngle(transform.localEulerAngles.y, m_targetRotation.y, ref m_velocityRotate.y, speedRotateSmooth);
                float angleZ = Mathf.SmoothDampAngle(transform.localEulerAngles.z, m_targetRotation.z, ref m_velocityRotate.z, speedRotateSmooth);

                transform.localEulerAngles = new Vector3(angleX, angleY, angleZ);
            }
        }

        private IEnumerator Cooldown()
        {
            canShot = false;

            yield return new WaitForSeconds(cooldown);

            canShot = true;
        }
    }
}
