using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public unsafe class Match : MonoBehaviour, IPacketReceiver
{
    public static Match Current;
    public Dictionary<long, Player> Players;

    void Awake()
    {
        Debug.Log("Match started");
        Current = this;
        Players = new Dictionary<long, Player>();
        Client.TCP.AddPacketReceiver(this);
        Client.UDP.AddPacketReceiver(this);
    }

    void Start()
    {
        AddPlayer(LocalPlayerInfo.ID, LocalPlayerInfo.Name);
        P_PlayerJoined playerJoined = new P_PlayerJoined()
        {
            id = LocalPlayerInfo.ID,
            name = LocalPlayerInfo.Name
        };
        Client.TCP.SendPacket(E_PACKET.PLAYER_JOINED, playerJoined);
    }

    private void OnGUI()
    {
        foreach (Player player in Players.Values)
        {
            if (player.ID != LocalPlayerInfo.ID)
            {
                Vector3 scpos = GameObject.Find("Player Camera").GetComponent<Camera>().WorldToScreenPoint(player.transform.position);
                if (scpos.z > 0)
                {
                    GUI.contentColor = Color.cyan;
                    GUI.Label(new Rect(scpos.x, Screen.height - scpos.y, 100, 25), player.Name);
                }
            }
        }
    }

    void OnDestroy()
    {
        Client.TCP.RemovePacketReceiver(this);
        Client.UDP.RemovePacketReceiver(this);
    }

    public unsafe void OnPacketReceived(Packet packet)
    {
        byte packetId = packet.pbase.packet_id;
        switch ((E_PACKET)packetId)
        {
            case E_PACKET.PLAYER_JOINED:
                P_PlayerJoined playerJoined = UnsafeCode.ByteArrayToStructure<P_PlayerJoined>(packet.data);
                AddPlayer(playerJoined.id, playerJoined.name);
                Debug.Log($"Player {playerJoined.name} has joined");
                break;

            case E_PACKET.CREATE_MATCH_PLAYER:
                P_CreateMatchPlayer matchPlayer = UnsafeCode.ByteArrayToStructure<P_CreateMatchPlayer>(packet.data);
                Player newPlayer = AddPlayer(matchPlayer.id, matchPlayer.name);
                if (newPlayer != null)
                {
                    newPlayer.transform.position = matchPlayer.position;
                    newPlayer.transform.rotation = matchPlayer.rotation;
                }
                break;

            case E_PACKET.PLAYER_LEFT:
                P_PlayerLeft playerLeft = UnsafeCode.ByteArrayToStructure<P_PlayerLeft>(packet.data);
                RemovePlayer(playerLeft.id);
                break;

            case E_PACKET.UPDATE_PLAYER_MOVEMENT:
                P_UpdatePlayerMovement updateMovement = UnsafeCode.ByteArrayToStructure<P_UpdatePlayerMovement>(packet.data);
                if (Players.TryGetValue(updateMovement.player_id, out Player player) && player != null)
                {
                    player.Movement.Move(updateMovement.motion);
                }
                break;

            default:
                break;

        }
    }

    private Player AddPlayer(long id, string playerName)
    {
        if (Players == null || Players.ContainsKey(id))
            return null;

        bool local = LocalPlayerInfo.ID == id;
        GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        playerObj.name = playerName;
        if (local)
        {
            GameObject cameraObject = new GameObject($"Player Camera");
            Camera playerCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<MouseLook>();
            playerCamera.transform.parent = playerObj.transform;
        }
        PlayerMovement playerMovement = playerObj.AddComponent<PlayerMovement>();
        playerMovement.Controller = playerObj.AddComponent<CharacterController>();
        Player player = playerObj.AddComponent<Player>();
        player.ID = id;
        player.Name = playerName;
        player.Movement = playerMovement;
        player.IsLocal = local;
        Players.Add(id, player);
        return player;
    }

    private void RemovePlayer(long id)
    {
        if (Players != null && Players.TryGetValue(id, out Player player) && player != null)
        {
            Destroy(player.gameObject);
            Players.Remove(id);
        }
    }
}
