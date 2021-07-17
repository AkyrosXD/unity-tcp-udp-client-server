using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
public struct PacketBase
{
    [MarshalAs(UnmanagedType.U1)]
    public byte packet_id;

    [MarshalAs(UnmanagedType.I2)]
    public short length;
}

public unsafe struct Packet
{
    public PacketBase pbase;
    public byte[] data;
}

public unsafe class NetworkClient
{
    private const int UDP_MAX_DATA_LENGTH = 512;
    private Socket socket;
    private readonly IPEndPoint endPoint;
    private readonly ProtocolType socketProtocol;
    private readonly SynchronizationContext synchronizationContext;
    private readonly List<IPacketReceiver> packetReceivers = new List<IPacketReceiver>();

    public event Action OnDisconnect;

    public NetworkClient(string ip, int port, ProtocolType protocol)
    {
        endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        socket = new Socket(endPoint.AddressFamily, (protocol == ProtocolType.Udp) ? SocketType.Dgram : SocketType.Stream, protocol);
        socketProtocol = protocol;
        synchronizationContext = SynchronizationContext.Current;
    }

    public void Start()
    {
        socket.Connect(endPoint);
        if (socketProtocol == ProtocolType.Tcp)
        {
            new Thread(ReadTcpDataThread).Start();
        }
        else if (socketProtocol == ProtocolType.Udp)
        {
            new Thread(ReadUdpDataThread).Start();
        }
    }

    private void ReadUdpDataThread()
    {
        byte[] clientBuffer = new byte[UDP_MAX_DATA_LENGTH];
        EndPoint ep = socket.RemoteEndPoint;
        while (socket != null)
        {
            int bytesReceived = 0;
            try
            {
                bytesReceived = socket.ReceiveFrom(clientBuffer, 0, UDP_MAX_DATA_LENGTH, SocketFlags.None, ref ep);
            }
            catch
            {
                break;
            }
            if (bytesReceived > 0)
            {
                PacketBase packetBase = default;
                Packet packet = default;
                packetBase.packet_id = clientBuffer[0];
                packetBase.length = BitConverter.ToInt16(clientBuffer, sizeof(byte));
                packet.pbase = packetBase;
                packet.data = UnsafeCode.SubArray(clientBuffer, sizeof(PacketBase), packetBase.length - sizeof(PacketBase));
                synchronizationContext.Post((object state) => { HandlePacket(packet); }, null);
            }
            else if (bytesReceived < 0)
            {
                break;
            }
            Thread.Sleep(50);
        }
        Close();
    }

    private void ReadTcpDataThread()
    {
        int offset = 0;
        byte[] packetBaseBuffer = new byte[sizeof(PacketBase)];
        byte[] clientBuffer = null;
        bool bBase = false;
        while (socket != null)
        {
            if (!bBase)
            {
                int packetBaseBytesReceived = 0;
                try
                {
                    packetBaseBytesReceived = socket.Receive(packetBaseBuffer, offset, sizeof(PacketBase) - offset, SocketFlags.None);
                }
                catch
                {
                    break;
                }
                offset += packetBaseBytesReceived;
                bBase = (offset == sizeof(PacketBase));
            }
            else
            {
                Packet packet = default;
                packet.pbase = UnsafeCode.ByteArrayToStructure<PacketBase>(packetBaseBuffer);
                if (clientBuffer == null)
                {
                    clientBuffer = new byte[packet.pbase.length];
                    Buffer.BlockCopy(packetBaseBuffer, 0, clientBuffer, 0, sizeof(PacketBase));
                }
                int bytesReceived = 0;
                try
                {
                    bytesReceived = socket.Receive(clientBuffer, offset, packet.pbase.length - offset, SocketFlags.None);
                }
                catch
                {
                    break;
                }
                if (bytesReceived > 0)
                {
                    offset += bytesReceived;
                    if (offset < packet.pbase.length)
                    {
                        continue;
                    }
                    packet.data = UnsafeCode.SubArray(clientBuffer, sizeof(PacketBase), packet.pbase.length - sizeof(PacketBase));
                    synchronizationContext.Post((object state) => { HandlePacket(packet); }, null);
                    clientBuffer = null;
                    offset = 0;
                    bBase = false;
                }
                else if (bytesReceived < 0)
                {
                    break;
                }
            }
        }
        Close();
        if (OnDisconnect != null)
        {
            synchronizationContext.Post((object state) => { OnDisconnect(); }, null);
        }
    }


    public void HandlePacket(Packet packet)
    {
        for (int i = 0; i < packetReceivers.Count; i++)
        {
            packetReceivers[i].OnPacketReceived(packet);
        }
    }

    private void SendData(E_PACKET packetId, byte[] data)
    {
        if (socket != null)
        {
            int sz = data.Length + sizeof(PacketBase);
            byte[] sizeInBytes = BitConverter.GetBytes((short)sz);
            byte[] buff = new byte[sz];
            buff[0] = (byte)packetId;
            Buffer.BlockCopy(sizeInBytes, 0, buff, 1, sizeInBytes.Length);
            Buffer.BlockCopy(data, 0, buff, sizeof(PacketBase), data.Length);
            try
            {
                if (socketProtocol == ProtocolType.Tcp)
                {
                    socket.Send(buff);
                }
                else
                {
                    socket.SendTo(buff, endPoint);
                }
            }
            catch { Close(); }
        }
    }

    public void SendPacket(E_PACKET packetId)
    {
        if (socket != null)
        {
            int sz = sizeof(PacketBase);
            byte[] buff = new byte[sz];
            buff[0] = (byte)packetId;
            byte[] sizeInBytes = BitConverter.GetBytes(sz);
            Buffer.BlockCopy(sizeInBytes, 0, buff, sizeof(byte), sizeInBytes.Length);
            try
            {
                socket.Send(buff);
            }
            catch { Close(); }
        }
    }

    public void SendPacket(E_PACKET packetId, object packet)
    {
        int size = Marshal.SizeOf(packet);
        byte* ptr = stackalloc byte[size];
        IntPtr ptr2 = (IntPtr)ptr;
        Marshal.StructureToPtr(packet, ptr2, true);
        byte[] data = new byte[size];
        Marshal.Copy(ptr2, data, 0, size);
        SendData(packetId, data);
    }

    public void AddPacketReceiver(IPacketReceiver item)
    {
        if (!packetReceivers.Contains(item))
        {
            packetReceivers.Add(item);
        }
    }

    public void RemovePacketReceiver(IPacketReceiver item)
    {
        packetReceivers.Remove(item);
    }

    public void Close()
    {
        if (socket != null)
        {
            socket.Dispose();
            socket = null;
        }
    }
}
