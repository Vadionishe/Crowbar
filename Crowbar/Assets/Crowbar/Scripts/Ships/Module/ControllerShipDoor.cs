using Mirror;
using UnityEngine;

namespace Crowbar.Ship
{
    public class ControllerShipDoor : NetworkBehaviour, IShipModule, IPickInfo
    {
        public ShipDoor door;

        public Color PickColor = Color.green;

        private Color m_colorMain;

        public void Pick()
        {
            m_colorMain = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = PickColor;
        }

        public void UnPick()
        {
            GetComponent<SpriteRenderer>().color = m_colorMain;
        }

        [Server]
        public void Use(NetworkIdentity usingCharacter)
        {
            door.SetState(!door.isOpen);
        }
    }
}