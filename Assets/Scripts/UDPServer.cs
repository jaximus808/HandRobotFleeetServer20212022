using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

public class UDPServer : MonoBehaviour
{
    // Start is called before the first frame update

    //going to need a python communicator object reference 
    //public HandRendererOne[] HandManagers;
    public GameObject sethand; 

    public static GameObject HandRenderer; 

    public static Dictionary<int, HandRendererOne[]> HandManagers = new Dictionary<int, HandRendererOne[]>();
    public static Dictionary<int, Client> ConnectedClients = new Dictionary<int, Client>();

    private static readonly List<Action> MainThreadQueue = new List<Action>();
    private static readonly List<Action> MainCopyThreadQueue = new List<Action>();
    private static bool actionToExecuteOnMainThread = false; 

    public delegate void Handler(int _fromClient, Packet _packet);

    public static Dictionary<int, Handler> packetHandlers;

    public bool active = false; 

    public int inPort = 8000; //port to receive data
    public int outPort = 8001; //port to send data

    private static UdpClient client; 
    private static IPEndPoint remoteEndPoint; 
    private static Thread receiveThread;

    public void SendData(string message)
    {
        try 
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch(Exception error)
        {
            Debug.Log(error.ToString());
        }
    }

    private void Awake()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Any, outPort);
        HandRenderer = sethand;
        client = new UdpClient(inPort);
        client.BeginReceive(ReceiveUDPData,null);
        //0 connect
        //1 read and handle vector data
        packetHandlers = new Dictionary<int, Handler>()
        {
            {0, PacketHandler.HandleNewConnection},
            {1, PacketHandler.HandleHand},
        };


        //receiveThread = new Thread(new ThreadStart(ReceiveData));
        //receiveThread.IsBackground = true; 
        //receiveThread.Start();
        Debug.Log("Listening");

    }

    public static Vector3[] ReadHandPoints(Packet packet)
    {
        Vector3[] data = new Vector3[21];
        for(int i = 0; i < 21; i++)
        {
            data[i] = packet.ReadVector3();
            Debug.Log(data[i]);
            //Debug.Log(data[i]);
        }
        return data;
    }

    public static void CreateClient(int _fromClient, IPEndPoint endpoint)
    {
        int _id = 0;
        foreach (KeyValuePair<int, Client> _client in ConnectedClients)
        {
            if ((_client.Key + 1) - _id == 1)
            {
                _id++;
            }
            else
            {
                break;
            }
        }
        ConnectedClients.Add(_id, new Client(_id));
        ConnectedClients[_id].udp.Connect(endpoint);
        HandRendererOne leftHand = Instantiate(HandRenderer, new Vector3(-2f, 10f, 3.2f), Quaternion.identity).GetComponent<HandRendererOne>();
        HandRendererOne rightHand = Instantiate(HandRenderer, new Vector3(-7.7f, 10f, 3.2f), Quaternion.identity).GetComponent<HandRendererOne>();
        HandRendererOne[] newHands = new HandRendererOne[2] { leftHand, rightHand };

        HandManagers.Add(_id, newHands);
        Debug.Log("New Client Created!");
        
        //maybe create a better way of doing this lol
        using (Packet packet = new Packet())
        {
            packet.Write(0);
            packet.Write(_id);
            ConnectedClients[_id].udp.SendData(packet);
        }
    }

    private void ReceiveUDPData(IAsyncResult _result)
    {
        
        try
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.EndReceive(_result,ref anyIP);
            client.BeginReceive(ReceiveUDPData, null);
                // bool hnads = Encoding.UTF8.GetString(data);
            Packet packet = new Packet(data, anyIP);
                //check for new client connection
            int id = packet.ReadInt();
            //if -1 means new connection 
            
            if(id == -1 )
            {
                    //will need to check if hand, or maybe attach to hand 
                    //new connection
                UDPServer.ExecuteOnMainThread(() =>
                {
                    UDPServer.packetHandlers[0](id, packet);
                });

            }
            else if(ConnectedClients[id].udp.endPoint.ToString() == anyIP.ToString() )
            {
                Debug.Log("sup");
                Debug.Log(id);
                Debug.Log("cock");
                ConnectedClients[id].udp.HandleData(packet);
                ProcessInput(packet);
            }
                //handle here 

                
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
        
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                client.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

    private void ProcessInput(Packet input)
    {
        // PROCESS INPUT RECEIVED STRING HERE

        if (!active) // First data arrived so tx started
        {
            active = true;
        }
    }

    private void FixedUpdate()
    {
        UpdateMain();
    }

    public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
        {
            Console.WriteLine("No action to execute on main thread!");
            return;
        }
        lock(MainThreadQueue)
        {
            MainThreadQueue.Add(_action);
            actionToExecuteOnMainThread = true; 
        }
    }

    public static void UpdateMain()
    {
        if(actionToExecuteOnMainThread)
        {
            MainCopyThreadQueue.Clear();
            lock(MainThreadQueue)
            {
                MainCopyThreadQueue.AddRange(MainThreadQueue);
                MainThreadQueue.Clear();
                actionToExecuteOnMainThread = false; 
            }
            for (int i = 0; i < MainCopyThreadQueue.Count; i++)
            {
                MainCopyThreadQueue[i]();
            }
        }
    }

    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }
}
