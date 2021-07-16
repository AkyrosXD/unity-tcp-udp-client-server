using System.Runtime.InteropServices;
using UnityEngine;

public enum E_PACKET
{
    PLAYER_NAME,
    PLAYER_NAME_SUCCESS,
    PLAYER_JOINED,
    CREATE_MATCH_PLAYER,
    PLAYER_MOVEMENT,
    UPDATE_PLAYER_MOVEMENT,
    SEND_CHAT_MESSAGE,
    RECEIVE_CHAT_MESSAGE,
    PLAYER_LEFT
}

[StructLayout(LayoutKind.Sequential, Size = 16)]
struct P_PlayerName
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string name;
}

[StructLayout(LayoutKind.Sequential, Size = 24)]
struct P_PlayerNameSuccess
{
    [MarshalAs(UnmanagedType.I8)]
    public long assigned_id;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string name;
}

[StructLayout(LayoutKind.Sequential, Size = 24)]
struct P_PlayerJoined
{
    [MarshalAs(UnmanagedType.I8)]
    public long id;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string name;
}

[StructLayout(LayoutKind.Sequential, Size = 56)]
struct P_CreateMatchPlayer
{
    [MarshalAs(UnmanagedType.I8)]
    public long id;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string name;

    [MarshalAs(UnmanagedType.Struct)]
    public Vector3 position;

    [MarshalAs(UnmanagedType.Struct)]
    public Quaternion rotation;
}

[StructLayout(LayoutKind.Sequential, Size = 32)]
struct P_PlayerMovement
{
    [MarshalAs(UnmanagedType.I8)]
    public long player_id;

    [MarshalAs(UnmanagedType.R4)]
    public float dx;

    [MarshalAs(UnmanagedType.R4)]
    public float dy;

    [MarshalAs(UnmanagedType.Struct)]
    public Quaternion rotation;
}

[StructLayout(LayoutKind.Sequential, Size = 40)]
struct P_UpdatePlayerMovement
{
    [MarshalAs(UnmanagedType.I8)]
    public long player_id;

    [MarshalAs(UnmanagedType.Struct)]
    public Quaternion rotation;

    [MarshalAs(UnmanagedType.Struct)]
    public Vector3 motion;

}

[StructLayout(LayoutKind.Sequential, Size = 64)]
struct P_SendChatMessage
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string message;
}

[StructLayout(LayoutKind.Sequential, Size = 80)]
struct P_ReceiveChatMessage
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string sender;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string message;
}

[StructLayout(LayoutKind.Sequential, Size = 24)]
struct P_PlayerLeft
{
    [MarshalAs(UnmanagedType.I8)]
    public long id;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string name;
}