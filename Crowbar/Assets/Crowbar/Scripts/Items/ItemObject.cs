using Mirror;
using System.Collections;
using UnityEngine;

namespace Crowbar.Item
{
    public abstract class ItemObject : WorldObject, IUse, IPickInfo
    {      
        public int damage;
        public float cooldownAttack;
        public float handedAngle;
        public bool handedForceAngle;
        public bool isAttacking;
        public bool onCooldown;

        public NetworkIdentity handedCharacter;
        public SpriteRenderer rendererItem;
        public Collider2D colliderItem;

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
        public virtual void Drop(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            PlayerInputForServer playerInput = usingCharacter.GetComponent<PlayerInputForServer>();

            canParenting = true;
            transform.parent = character.transform.parent;
            character.hand.itemObject = null;
            handedCharacter = null;
            colliderItem.isTrigger = false;
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            playerInput.onPushE.RemoveListener(UseItem);

            RpcDropItem(usingCharacter);
        }

        [Server]
        public virtual void Grab(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            UsingComponent usingComponent = usingCharacter.GetComponent<UsingComponent>();

            if (character.hand.itemObject == null && !character.isBusy && usingComponent.canItemGrab) 
            {
                canParenting = false;
                transform.parent = character.hand.handParentPoint;
                transform.localPosition = Vector3.zero;
                character.hand.itemObject = this;
                handedCharacter = usingCharacter;
                colliderItem.isTrigger = true;
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                StartCoroutine(WaitToListenUse());

                if (handedForceAngle)
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    transform.localEulerAngles = new Vector3(0, 0, handedAngle);
                }

                usingComponent.SetCooldownGrab();
                RpcGrabItem(usingCharacter);
            }
        }

        [Server]
        public virtual void UseItem()
        {
            if (handedCharacter == null || !handedCharacter.GetComponent<UsingComponent>().canItemGrab)
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

        public void Attack()
        {
            StartCoroutine(Cooldown());
        }

        protected void CheckToDestroy()
        {
            Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 600f, LayerMask.GetMask("Player"));

            if (players.Length == 0)
                NetworkServer.Destroy(gameObject);
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
