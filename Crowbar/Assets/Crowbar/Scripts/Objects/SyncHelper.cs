namespace Crowbar.Net
{
    using UnityEngine;
    using Mirror;
    using Crowbar;
    using Crowbar.Item;

    public class SyncHelper : NetworkBehaviour
    {
        private static SyncHelper instance;

        public static void SetParent(GameObject syncObject, NetworkIdentity parent)
        {
            syncObject.transform.SetParent((parent != null) ? parent.transform : null);
        }

        public static void SetParentTransform(GameObject syncObject, Transform parent)
        {
            syncObject.transform.SetParent(parent.transform);
        }

        [Server]
        public static void SyncObject(NetworkConnection connection, WorldObject syncObject)
        {
            if (syncObject.transform.parent == null)
            {
                instance.TargetSetParent(connection, syncObject.gameObject, null, syncObject.transform.position, syncObject.transform.eulerAngles, syncObject.transform.localScale, -1);
            }
            else
            {
                NetworkIdentity identity = syncObject.transform.parent.GetComponent<NetworkIdentity>();
                ItemObject itemObject = (ItemObject)syncObject;

                //if (itemObject != null)
                //    instance.TargetSetVisible(connection, syncObject.gameObject, itemObject.render.enabled);

                if (identity == null)
                    identity = syncObject.transform.parent.GetComponentInParent<NetworkIdentity>();

                Character character = identity.GetComponent<Character>();

                //if (itemObject != null && character != null)
                //{
                //    int indexSlot = character.slotsCharacter.IndexOf(character.slotsCharacter.Find(x => x.item == itemObject.gameObject));

                //    instance.TargetSetParent(connection, syncObject.gameObject, identity, syncObject.transform.localPosition, syncObject.transform.localEulerAngles, syncObject.transform.localScale, indexSlot);
                //}
                //else
                //{
                    instance.TargetSetParent(connection, syncObject.gameObject, identity, syncObject.transform.localPosition, syncObject.transform.localEulerAngles, syncObject.transform.localScale, -1);
                //}
            }
        }

        [Server]
        public static void SyncBag(NetworkConnection connection, ItemObject itemObject)
        {
            IBag bag = itemObject.GetComponent<IBag>();

            //foreach (ItemObject item in bag.items)
            //    instance.TargetSyncBag(connection, itemObject.instanceId, itemObject.gameObject, item.gameObject, item.instanceId);
        }

        private void Start()
        {
            instance = this;
        }

        [TargetRpc]
        private void TargetSyncBag(NetworkConnection connection, int bagId, GameObject bagItem, GameObject item, int id)
        {
            IBag bag = bagItem.GetComponent<IBag>();

            //bag.RpcAddClientItem(bagId, id, item, "");
        }

        [TargetRpc]
        private void TargetSetParent(NetworkConnection connection, GameObject syncObject, NetworkIdentity parent, Vector3 position, Vector3 rotation, Vector3 scale, int indexSlot)
        {
            SyncPosition syncPosition = syncObject.transform.GetComponent<SyncPosition>();

            if (parent != null)
            {
                Character character = parent.GetComponent<Character>();

                if (character != null && indexSlot > -1)
                {
                    //SetParentTransform(syncObject, character.slotsCharacter[indexSlot].slotPosition);

                    syncPosition.syncPosition = false;
                    syncPosition.syncRotation = false;

                    syncObject.transform.localPosition = position;
                    syncObject.transform.localEulerAngles = rotation;
                    syncObject.transform.localScale = scale;

                    return;
                }
            }

            SetParent(syncObject, parent);
        }

        [TargetRpc]
        private void TargetSetVisible(NetworkConnection connection, GameObject syncObject, bool isVisible)
        {
            ItemObject itemObject = syncObject.GetComponent<ItemObject>();

            //if (itemObject != null)
            //    itemObject.render.enabled = isVisible;
        }
    }
}
