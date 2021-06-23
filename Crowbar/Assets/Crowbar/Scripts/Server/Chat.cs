using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace Crowbar
{
    public class Chat : NetworkBehaviour
    {
        public static bool lockInput;

        public Text textChat;
        public InputField inputChat;
        public KeyCode keyChat = KeyCode.Return;

        public int historyMessageCount = 10;
        public float timeVisibleTextChat = 5f;

        public List<string> historyMessage;

        [Command(ignoreAuthority = true)]
        public void CmdSendMessage(string message, NetworkIdentity sender)
        {
            RpcSendMessage($"[{sender.GetComponent<Character>().nameCharacter}]: {message}");
        }

        [ClientRpc]
        public void RpcSendMessage(string message)
        {
            historyMessage.Add(message);

            if (historyMessage.Count > historyMessageCount)
                historyMessage.RemoveAt(0);

            ShowMessages();
        }

        private void ShowMessages()
        {
            textChat.text = string.Empty;

            for (int i = 0; i < historyMessage.Count; i++)
                textChat.text += historyMessage[i] + "\n";

            StopAllCoroutines();
            StartCoroutine(VisibleTextChat());
        }

        private IEnumerator VisibleTextChat()
        {
            textChat.color = Color.white;

            yield return new WaitForSeconds(timeVisibleTextChat);

            for (int i = 0; i < 50; i++)
            {
                yield return new WaitForFixedUpdate();

                textChat.color = new Color(1, 1, 1, textChat.color.a - Time.fixedDeltaTime);
            }
        }

        private void Awake()
        {
            historyMessage = new List<string>();
        }

        private void Update()
        {
            if (!isServer)
            {
                if (Input.GetKeyDown(keyChat))
                {
                    if (inputChat.gameObject.activeSelf)
                    {
                        if (inputChat.text != string.Empty)
                            CmdSendMessage(inputChat.text, GameUI.localStats.GetComponent<NetworkIdentity>());

                        inputChat.DeactivateInputField();
                        inputChat.gameObject.SetActive(false);

                        lockInput = false;
                    }
                    else
                    {
                        inputChat.gameObject.SetActive(true);
                        inputChat.ActivateInputField();

                        lockInput = true;
                    }

                    inputChat.text = string.Empty;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    inputChat.DeactivateInputField();
                    inputChat.gameObject.SetActive(false);

                    inputChat.text = string.Empty;
                    lockInput = false;
                }
            }
        }
    }
}
