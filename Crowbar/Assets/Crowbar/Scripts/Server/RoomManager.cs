using Crowbar.Server;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Crowbar
{
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager roomManager;

        public GameObject roomContainer;
        public RoomInstance prefabRoom;
        public Text infoPlayers;
        public List<RoomInstance> rooms;
        
        public void UpdatePlayerRoom(string info)
        {
            infoPlayers.text = info;
        }

        public void SpawnRoom(int id, string info)
        {
            RoomInstance room = Instantiate(prefabRoom, roomContainer.transform);

            room.id = id;
            room.text.text = info;

            rooms.Add(room);
        }

        public void UpdateRoom(int id, string newInfo)
        {
            RoomInstance room = rooms.Find(r => r.id == id);

            if (room != null)
                room.text.text = newInfo;
        }

        public void RemoveRoom(int id)
        {
            RoomInstance room = rooms.Find(r => r.id == id);

            if (room != null)
            {
                rooms.Remove(room);
                Destroy(room.gameObject);
            }
        }

        private void Awake()
        {
            roomManager = this;
            rooms = new List<RoomInstance>();
        }
    }
}
