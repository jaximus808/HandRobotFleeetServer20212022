using System.Collections;
using System.Collections.Generic;
using System; 
using UnityEngine;

public static class PacketHandler 
{
    //packet 0
    public static void HandleNewConnection(int _fromClient, Packet _packet)
    {
        UDPServer.CreateClient(_fromClient,_packet.remoteEndPoint);
        
    }

    //packet 1
    public static void HandleHand(int _fromClient, Packet _packet )
    {
        bool hands = _packet.ReadBool();
        if (hands)
        {
            //can make this smaller
            int handCount = _packet.ReadInt();
            Debug.Log(handCount);
            bool right = _packet.ReadBool();
            Debug.Log(handCount);
            int side = 0;
            if (right) side = 1;
            //test floatsZyy
            Vector3[] handPoints1 = UDPServer.ReadHandPoints(_packet);
            //would then apply to hands but do later rn;
            UDPServer.HandManagers[_fromClient][side].UpdateHands(handPoints1);
            if (handCount == 2)
            {
                Vector3[] handPoints2 = UDPServer.ReadHandPoints(_packet);
                UDPServer.HandManagers[_fromClient][Math.Abs(side - 1)].UpdateHands(handPoints2);
            }
        }
    }
}
