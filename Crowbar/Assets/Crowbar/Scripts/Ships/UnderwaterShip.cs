using UnityEngine;
using Mirror;
using System.Collections.Generic;

namespace Crowbar.Ship
{
    public class UnderwaterShip : WorldObject
    {
        [SyncVar]
        public bool isMove;

        public Transform shipPhysic;
        public List<Collider2D> collidersShip;
        public List<MotorRotate> motorRotates;
        public GameObject forwardShip;
        public GameObject forwardShipVisibleAlways;
        public Place parentingShip;
        public AudioSource audioSource;

        public Water water;
        public Transform crackParent;
        public GameObject crackPrefab;
        public string nameParentCracks = "Cracks";

        public int maxCountCracks = 12;

        public Vector2 LeftUpPointShip, RightDownPointShip;

        [Server]
        public void SetMotorStateServer(MotorRotate.Side side, bool isRotate)
        {            
            for (int i = 0; i < motorRotates.Count; i++)
            {
                if (motorRotates[i].side == side)
                {
                    if (motorRotates[i].isRotate != isRotate)
                        RpcSetMotorState(i, isRotate);

                    motorRotates[i].isRotate = isRotate;

                    if (!isRotate)
                        motorRotates[i].SetStandRotate();
                }
            }
        }

        [ClientRpc]
        public void RpcSetMotorState(int indexMotor, bool isRotate)
        {
            motorRotates[indexMotor].isRotate = isRotate;

            if (!isRotate)
                motorRotates[indexMotor].SetStandRotate();
        }

        [Server]
        [ContextMenu("AddCrack")]
        public void AddCrack()
        {
            if (Crack.count < maxCountCracks)
            {
                GameObject crackObject = Instantiate(crackPrefab, transform.position, Quaternion.identity, crackParent);
                Crack crack = crackObject.GetComponent<Crack>();

                crackObject.transform.localPosition = GetPositionCrack();
                crack.waterPlace = water;

                NetworkServer.Spawn(crackObject);
                crackObject.transform.localPosition = new Vector3(crackObject.transform.localPosition.x, crackObject.transform.localPosition.y, 0);

                crack.RpcSyncPosition(crackObject.transform.localPosition, GetComponent<NetworkIdentity>(), nameParentCracks);

                foreach (Character character in parentingShip.GetComponentsInChildren<Character>())
                    character.TargetCameraShake(character.netIdentity.connectionToClient, 0.5f, 0.4f);

                Crack.count++;
            }
        }

        public void VisibleInterier(WorldObject worldObject, bool isEnterShip)
        {
            if (worldObject.canParenting && worldObject.isLocalPlayer)
            {
                float newLayer = (isEnterShip) ? LayerManager.instance.shipLayerIn : LayerManager.instance.shipLayerOut;

                forwardShip.SetActive(!isEnterShip);

                transform.position = new Vector3(transform.position.x, transform.position.y, newLayer);
                forwardShipVisibleAlways.transform.localPosition = new Vector3(forwardShipVisibleAlways.transform.localPosition.x,
                    forwardShipVisibleAlways.transform.localPosition.y,
                    (isEnterShip) ? -LayerManager.instance.shipLayerIn : forwardShip.transform.localPosition.z);
            }
        }

        private Vector3 GetPositionCrack()
        {
            Vector3 posCrack = new Vector3();

            posCrack.z = 0;
            posCrack.x = Random.Range(LeftUpPointShip.x, RightDownPointShip.x);
            posCrack.y = Random.Range(LeftUpPointShip.y, RightDownPointShip.y);

            return posCrack;
        }

        private void Start()
        {
            audioSource.volume = Settings.volume;

            if (isServer)
            {
                collidersShip = new List<Collider2D>();

                foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
                    if (LayerMask.LayerToName(collider.gameObject.layer) == "GroundCollision")
                        collidersShip.Add(collider);

                foreach (Collider2D collider in collidersShip)
                    Physics2D.IgnoreCollision(collider, shipPhysic.GetComponent<Collider2D>(), true);
            }
            else
            {
                parentingShip.onParenting.AddListener(VisibleInterier);
                Destroy(shipPhysic.gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (isServer)
            {
                transform.position = shipPhysic.position;
                transform.eulerAngles = shipPhysic.eulerAngles;
            }
            else
            {
                if (isMove)
                {
                    if (!audioSource.isPlaying)
                        audioSource.Play();
                }
                else
                {
                    if (audioSource.isPlaying)
                        audioSource.Stop();
                }
            }
        }
    }
}