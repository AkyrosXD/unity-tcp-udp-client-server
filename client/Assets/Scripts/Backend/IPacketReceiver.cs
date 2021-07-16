using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPacketReceiver
{
    unsafe void OnPacketReceived(Packet packet);
}
