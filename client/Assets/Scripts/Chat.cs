using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chat : MonoBehaviour, IPacketReceiver
{
    private const float CHAT_WINDOW_HEIGHT = 250f;

    private bool bShowInput = false;
    private bool bShowMessages = false;
    private List<string> messages = new List<string>();
    private string currentMessage = string.Empty;
    private Vector3 chatScroll = new Vector2(0, CHAT_WINDOW_HEIGHT);
    private IEnumerator showMessagesCorutine;
    private GUIStyle chatMessageStyle;

    void Start()
    {
        Client.TCP.AddPacketReceiver(this);
    }

    void ScrollToBottom()
    {
        chatScroll.y = CHAT_WINDOW_HEIGHT;
    }

    void OnGUI()
    {
        bool enter = Event.current.type == EventType.KeyDown && (Event.current.character == '\n');
        bShowInput ^= enter;
        GUI.backgroundColor = Color.clear;
        GUI.Window(1337, new Rect(10, Screen.height - CHAT_WINDOW_HEIGHT, 400, CHAT_WINDOW_HEIGHT), ChatWindow, string.Empty);
        if (enter)
        {
            ScrollToBottom();
            if (!string.IsNullOrWhiteSpace(currentMessage) && !string.IsNullOrWhiteSpace(currentMessage))
            {
                P_SendChatMessage chatMessage = default;
                chatMessage.message = currentMessage;
                currentMessage = string.Empty;
                Client.TCP.SendPacket(E_PACKET.SEND_CHAT_MESSAGE, chatMessage);
            }
        }
    }

    void ChatWindow(int id)
    {
        // the scrollbar does not function properly
        // I will fix it in the future
        chatScroll = GUILayout.BeginScrollView(chatScroll);
        if (bShowMessages || bShowInput)
        {
            if (chatMessageStyle == null)
            {
                chatMessageStyle = new GUIStyle(GUI.skin.label);
                chatMessageStyle.fontStyle = FontStyle.Bold;
            }
            for (int i = 0; i < messages.Count; i++)
            {
                GUILayout.Label(messages[i], chatMessageStyle);
            }
        }
        GUILayout.EndScrollView();
        if (bShowInput)
        {
            GUI.SetNextControlName("Message Input");
            currentMessage = GUILayout.TextField(currentMessage, 64, Array.Empty<GUILayoutOption>());
            GUI.FocusControl("Message Input");
        }
    }

    IEnumerator ShowMessages()
    {
        bShowMessages = true;
        yield return new WaitForSeconds(5);
        bShowMessages = false;
    }

    void AddMessage(string message)
    {
        messages.Add(message);
        if (messages.Count > 64)
        {
            messages.RemoveAt(0);
        }
        if (bShowMessages && showMessagesCorutine != null)
        {
            StopCoroutine(showMessagesCorutine);
        }
        showMessagesCorutine = ShowMessages();
        StartCoroutine(showMessagesCorutine);
        ScrollToBottom();
    }

    public void OnPacketReceived(Packet packet)
    {
        byte packetId = packet.pbase.packet_id;
        switch ((E_PACKET)packetId)
        {
            case E_PACKET.RECEIVE_CHAT_MESSAGE:
                P_ReceiveChatMessage chatMessage = UnsafeCode.ByteArrayToStructure<P_ReceiveChatMessage>(packet.data);
                string color = LocalPlayerInfo.Name == chatMessage.sender ? "lime" : "red";
                AddMessage($"<color={color}>[{chatMessage.sender}] {chatMessage.message}</color>");
                break;

            case E_PACKET.PLAYER_JOINED:
                P_PlayerJoined playerJoined = UnsafeCode.ByteArrayToStructure<P_PlayerJoined>(packet.data);
                AddMessage($"<color=blue>[Game] {playerJoined.name} has joined</color>");
                break;

            case E_PACKET.PLAYER_LEFT:
                P_PlayerLeft playerLeft = UnsafeCode.ByteArrayToStructure<P_PlayerLeft>(packet.data);
                AddMessage($"<color=blue>[Game] {playerLeft.name} has left</color>");
                break;

            default:
                break;
        }
    }

    void OnDestroy()
    {
        Client.TCP.RemovePacketReceiver(this);
    }
}
