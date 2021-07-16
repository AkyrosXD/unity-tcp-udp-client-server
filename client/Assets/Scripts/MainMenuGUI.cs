using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuGUI : MonoBehaviour, IPacketReceiver
{
    void Awake()
    {
        Client.Start();
    }

    // Start is called before the first frame update
    void Start()
    {
        Client.TCP.AddPacketReceiver(this);
    }

    public unsafe void OnPacketReceived(Packet packet)
    {
        byte packetId = packet.pbase.packet_id;
        switch ((E_PACKET)packetId)
        {
            case E_PACKET.PLAYER_NAME_SUCCESS:
                P_PlayerNameSuccess playerNameSuccess = UnsafeCode.ByteArrayToStructure<P_PlayerNameSuccess>(packet.data);
                LocalPlayerInfo.ID = playerNameSuccess.assigned_id;
                LocalPlayerInfo.Name = playerNameSuccess.name;
                SceneManager.LoadSceneAsync("Scenes/MatchScene", LoadSceneMode.Single);
                break;
        }
    }

    public void OnDestroy()
    {
        Client.TCP.RemovePacketReceiver(this);
    }

    public void OnJoinButtonClick()
    {
        string inputName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        P_PlayerName playerName = default;
        playerName.name = inputName;
        Client.TCP.SendPacket(E_PACKET.PLAYER_NAME, playerName);
    }
}
