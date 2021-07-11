using UnityEngine;
using UnityEngine.Events;
using Mirror;
using Crowbar.Ship;

namespace Crowbar
{
    public class Place : NetworkBehaviour
    {
        public class EventParenting : UnityEvent<WorldObject, bool> { }
        public EventParenting onParenting;

        public Water water;
        public OxygenStorage oxygen;
        public Transform target;

        public AudioClip placeSound;

        public bool openAir;
        public float positionZ;
        public int priority;

        private enum ActionTrigger
        {
            Enter,
            Exit
        }

        private void Awake()
        {
            onParenting = new EventParenting();
        }

        private void Update()
        {
            if (target != null)
            {
                transform.position = target.position;
                transform.rotation = target.rotation;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            OnParenting(collision.gameObject, ActionTrigger.Enter);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            OnParenting(collision.gameObject, ActionTrigger.Exit);
        }

        private void OnParenting(GameObject syncObject, ActionTrigger actionTrigger)
        {
            WorldObject worldObject = syncObject.transform.GetComponent<WorldObject>();
            SyncPosition syncPosition = syncObject.transform.GetComponent<SyncPosition>();
            NetworkIdentity syncObjectIdentity = null;
            NetworkIdentity parentIdentity = null;

            if (actionTrigger == ActionTrigger.Enter)
                parentIdentity = netIdentity;

            if (IsValidToParent(syncObject, actionTrigger, out syncObjectIdentity))
            {
                onParenting.Invoke(worldObject, parentIdentity != null);

                if (isServer)
                {
                    if (syncPosition.isServerObject && worldObject.canParenting)
                    {
                        SyncHelper.SetParent(worldObject.gameObject, parentIdentity);
                    }
                }
                else
                {
                    if (syncObjectIdentity.isLocalPlayer && syncObject.CompareTag("Player"))
                    {
                        SyncHelper.SetParent(worldObject.gameObject, parentIdentity);
                    }
                }

                syncObject.transform.localPosition = new Vector3(worldObject.transform.localPosition.x, worldObject.transform.localPosition.y, positionZ);
            }
        }

        private bool IsValidToParent(GameObject syncObject, ActionTrigger actionTrigger, out NetworkIdentity syncObjectIdentity)
        {           
            WorldObject worldObject = syncObject.transform.GetComponent<WorldObject>();
            SyncPosition syncPosition = syncObject.transform.GetComponent<SyncPosition>();

            syncObjectIdentity = null;

            if (syncObject.transform.parent == transform && actionTrigger == ActionTrigger.Enter)
                return false;

            if ((worldObject != null && !worldObject.canParenting) || worldObject == null)
                return false;

            if (actionTrigger == ActionTrigger.Enter)
            {
                if (gameObject.transform.parent != null)
                    if (gameObject.transform.parent.GetComponent<Place>() != null)
                        if (gameObject.transform.parent.GetComponent<Place>().priority >= priority)
                            return false;
            }

            if (!syncObject.TryGetComponent(out syncObjectIdentity) || syncPosition == null || worldObject == null)
                return false;

            return true;
        }
    }
}

