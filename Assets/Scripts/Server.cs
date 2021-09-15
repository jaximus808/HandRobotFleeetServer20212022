using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Connections {get;private set;}
    public static int Port {get;private set;}
    public static Dictionary<int,Client> clients = new Dictionary<int, Client>();
    public delegate void PacketsHandler(int _fromClient, Packets _packet);
    public static Dictionary<int,PacketsHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener; 
    

    // Start is called before the first frame update
    public static void Start(int _port)
    {
        MaxPlayers = 1;
        Port = _port;

        Debug.Log("Server Starting");
        CreateServerData();
    }

    // Update is called once per frame
    private static void CreateServerData()
    {
        
    }
}
