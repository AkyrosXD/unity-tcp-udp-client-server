using System.Net.Sockets;
using UnityEngine;

public static unsafe class Client
{
    private const string IP = "127.0.0.1";
    public static NetworkClient TCP = new NetworkClient(IP, 5004, ProtocolType.Tcp);
    public static NetworkClient UDP = new NetworkClient(IP, 5025, ProtocolType.Udp);

    public static void Start()
    {
        TCP.Start();
        UDP.Start();
        TCP.OnDisconnect += OnDisconnect;
        Application.wantsToQuit += OnApplicationQuit;
    }

    private static bool OnApplicationQuit()
    {
        Close();
        return true;
    }

    private static void OnDisconnect()
    {
        // do stuff
        // maybe display a message or something
    }

    public static void Close()
    {
        if (TCP != null)
        {
            TCP.Close();
        }
        if (UDP != null)
        {
            UDP.Close();
        }
    }
}
