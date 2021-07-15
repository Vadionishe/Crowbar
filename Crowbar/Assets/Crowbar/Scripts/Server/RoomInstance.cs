using UnityEngine;
using TMPro;

namespace Crowbar.Server
{
    public class RoomInstance : MonoBehaviour
    {
        public int id;
        public TextMeshProUGUI text;

        public void GoToRoom()
        {
            ClientMenu.localPlayerInstance.GoToRoom(id);
        }
    }
}
