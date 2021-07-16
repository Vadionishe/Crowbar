using Crowbar.System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Crowbar.Server
{
    public class ChatRoom : MonoBehaviour
    {
        public Text textChat;
        public InputField inputChat;
        public KeyCode keyChat = KeyCode.Return;
        public int historyMessageCount = 10;
        public List<string> historyMessage;

        public void SendMessageToRoom(string message)
        {
            ClientMenu.localPlayerInstance.SendMessageToRoom(message);
        }

        public void ReceiveMessage(string message)
        {
            historyMessage.Add(message);

            if (historyMessage.Count > historyMessageCount)
                historyMessage.RemoveAt(0);

            FlashWindow.Flash();
            ShowMessages();
        }

        private void ShowMessages()
        {
            textChat.text = string.Empty;

            for (int i = 0; i < historyMessage.Count; i++)
                textChat.text += historyMessage[i] + "\n";
        }

        private void Awake()
        {
            historyMessage = new List<string>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(keyChat))
            {
                if (inputChat.gameObject.activeSelf)
                {
                    if (inputChat.text != string.Empty)
                        SendMessageToRoom(inputChat.text);

                    inputChat.DeactivateInputField();
                    inputChat.gameObject.SetActive(false);
                }
                else
                {
                    inputChat.gameObject.SetActive(true);
                    inputChat.ActivateInputField();

                }

                inputChat.text = string.Empty;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                inputChat.DeactivateInputField();
                inputChat.gameObject.SetActive(false);

                inputChat.text = string.Empty;
            }
        }
    }
}
