﻿using Mirror;
using System.Collections;
using UnityEngine;

namespace Crowbar.Item
{
    public abstract class ItemObject : WorldObject, IUse, IPickInfo
    {
        public bool isFall = true;
        public bool dropFixedAngle = true;
        public float speedDown;
        public float offsetLanded;

        public int damage;
        public float cooldownAttack;
        public float handedAngle;
        public bool handedForceAngle;
        public bool isAttacking;
        public bool onCooldown;

        public NetworkIdentity handedCharacter;
        public SyncPosition syncPosition;
        public SpriteRenderer rendererItem;
        public Collider2D colliderItem;
        public Rigidbody2D rigidbodyItem;

        private Color PickColor = Color.green;
        private Color m_colorMain = Color.white;

        public void Pick()
        {
            if (rendererItem != null)
                rendererItem.color = PickColor;
        }

        public void UnPick()
        {
            if (rendererItem != null)
                rendererItem.color = m_colorMain;
        }

        [ClientRpc]
        public void RpcGrabItem(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            SyncPosition syncPosition = GetComponent<SyncPosition>();

            transform.parent = character.hand.handParentPoint;
            transform.localPosition = Vector3.zero;
            syncPosition.syncPosition = false;
            syncPosition.syncRotation = false;
            character.hand.itemObject = this;
            handedCharacter = usingCharacter;

            if (handedForceAngle)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                transform.localEulerAngles = new Vector3(0, 0, handedAngle);
            }
        }

        [ClientRpc]
        public void RpcDropItem(NetworkIdentity usingCharacter)
        {
            if (usingCharacter != null)
            {
                Character character = usingCharacter.GetComponent<Character>();

                character.hand.itemObject = null;
            }
            
            SyncPosition syncPosition = GetComponent<SyncPosition>();

            syncPosition.syncPosition = true;
            syncPosition.syncRotation = true;
            transform.parent = null;         
            handedCharacter = null;
        }

        [Server]
        public virtual void Drop(NetworkIdentity usingCharacter, float force, Vector2 direction, Vector2 position)
        {
            Character character = usingCharacter.GetComponent<Character>();
            PlayerInputForServer playerInput = usingCharacter.GetComponent<PlayerInputForServer>();
            
            canParenting = true;
            syncPosition.trueUpdate = true;
            transform.parent = character.transform.parent;
            character.hand.itemObject = null;
            handedCharacter = null;
            colliderItem.isTrigger = false;
            isFall = true;
            playerInput.onPushE.RemoveListener(UseItem);

            if (dropFixedAngle)
                transform.eulerAngles = Vector2.zero;

            RpcDropItem(usingCharacter);
        }

        [Server]
        public virtual void Grab(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();

            if (character.hand.itemObject == null && !character.isBusy) 
            {
                canParenting = false;
                transform.parent = character.hand.handParentPoint;
                transform.localPosition = Vector3.zero;
                character.hand.itemObject = this;
                handedCharacter = usingCharacter;
                colliderItem.isTrigger = true;
                StartCoroutine(WaitToListenUse());

                if (handedForceAngle)
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    transform.localEulerAngles = new Vector3(0, 0, handedAngle);
                }

                RpcGrabItem(usingCharacter);
            }
        }

        [Server]
        public virtual void UseItem()
        {
            if (handedCharacter == null || !handedCharacter.GetComponent<UsingComponent>().canUse)
                return;
        }

        [Server]
        public virtual void Initialize()
        {
        }

        [Server]
        public void Use(NetworkIdentity usingCharacter)
        {
            if (handedCharacter == null)
                Grab(usingCharacter);
        }

        [Server]
        public virtual void CheckValidSpawn()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, LayerMask.GetMask("GroundCollision"));

            if (new Vector2(hit.point.x, hit.point.y) == new Vector2(transform.position.x, transform.position.y))
                NetworkServer.Destroy(gameObject);
        }

        public virtual void CheckToDestroy()
        {
            Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 600f, LayerMask.GetMask("Player"));

            if (players.Length == 0)
                NetworkServer.Destroy(gameObject);
        }

        public virtual void Fall()
        {
            if (handedCharacter == null)
            {
                if (isFall)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f, LayerMask.GetMask("GroundCollision"));

                    if (hit.collider != null)
                    {                       
                        transform.position = hit.point + Vector2.up * offsetLanded;

                        isFall = false;
                    }
                    else
                    {
                        transform.position += Vector3.down * speedDown * Time.deltaTime;
                    }
                }
                else
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, LayerMask.GetMask("GroundCollision"));

                    if (hit.collider == null)
                        isFall = true;
                }
            }
        }

        public virtual void CheckToSleep()
        {
            if (rigidbodyItem.velocity.y < 1f && !rigidbodyItem.isKinematic)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f, LayerMask.GetMask("GroundCollision"));

                if (hit.collider != null)
                {
                    rigidbodyItem.isKinematic = true;
                    syncPosition.trueUpdate = false;
                }
            }
        }

        public void SetCooldown()
        {
            StartCoroutine(Cooldown());
        }

        private IEnumerator Cooldown()
        {
            onCooldown = true;

            yield return new WaitForSeconds(cooldownAttack);

            onCooldown = false;
        }

        private IEnumerator WaitToListenUse()
        {
            yield return new WaitForSeconds(0.4f);

            if (handedCharacter != null)
                handedCharacter.GetComponent<PlayerInputForServer>().onPushE.AddListener(UseItem);
        }

        private void OnDestroy()
        {
            if (isServer)
            {
                if (handedCharacter != null)
                {
                    PlayerInputForServer playerInput = handedCharacter.GetComponent<PlayerInputForServer>();

                    playerInput.onPushE.RemoveListener(UseItem);
                }
            }
        }
    }
}
