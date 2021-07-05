using Mirror;
using UnityEngine;

namespace Crowbar.Ship
{
    public class ControllerShipDoor : NetworkBehaviour, IShipModule, IPickInfo
    {
        public ShipDoor door;

        public Color PickColor = Color.green;

        private Color m_colorMain = Color.white;

        public void Pick()
        {
            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = PickColor;
        }

        public void UnPick()
        {
            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = m_colorMain;
        }

        [Server]
        public void Use(NetworkIdentity usingCharacter)
        {
            door.SetState(!door.isOpen);
        }
    }
}