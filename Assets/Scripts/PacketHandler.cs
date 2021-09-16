using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketHandler 
{
    // Maybe deprecate
    public static Packet CreatePacke(byte[] data)
    {
        return new Packet(data);
    }
}
