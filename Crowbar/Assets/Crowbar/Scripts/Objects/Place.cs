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
            if (syncObject.transform.parent == transform && actionTrigger == ActionTrigger.Enter)
                return;

            WorldObject worldObject = syncObject.transform.GetComponent<WorldObject>();
            
            if ((worldObject != null && !worldObject.canParenting) || worldObject == null)
                return;

            NetworkIdentity syncObjectIdentity = null;
            NetworkIdentity parentIdentity = null;            
            SyncPosition syncPosition = syncObject.transform.GetComponent<SyncPosition>();

            if (actionTrigger == ActionTrigger.Enter)
            {
                parentIdentity = netIdentity;

                if (gameObject.transform.parent != null)
                    if (gameObject.transform.parent.GetComponent<Place>() != null)
                        if (gameObject.transform.parent.GetComponent<Place>().priority >= priority)
                            return;
            }

            if (!syncObject.TryGetComponent(out syncObjectIdentity) || syncPosition == null || worldObject == null)
                return;

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
}

