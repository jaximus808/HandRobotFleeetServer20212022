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

        //0 connect
        //1 read and handle vector data
        packetHandlers = new Dictionary<int, Handler>()
        {
            {0, PacketHandler.HandleNewConnection},
            {1, PacketHandler.HandleHand},
        };

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true; 
        receiveThread.Start();
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

    public static void CreateClient(int _fromClient)
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
        HandRendererOne leftHand = Instantiate(HandRenderer, new Vector3(-2f, 10f, 3.2f), Quaternion.identity).GetComponent<HandRendererOne>();
        HandRendererOne rightHand = Instantiate(HandRenderer, new Vector3(-7.7f, 10f, 3.2f), Quaternion.identity).GetComponent<HandRendererOne>();
        HandRendererOne[] newHands = new HandRendererOne[2] { leftHand, rightHand };

        HandManagers.Add(_id, newHands);
        Debug.Log("New Client Created!");
    }

    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                // bool hnads = Encoding.UTF8.GetString(data);
                Packet packet = new Packet(data);
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

                    ConnectedClients[id].udp.HandleData(packet);
                    //bool hands = packet.ReadBool();
                    //if (hands)
                    //{
                    //    //can make this smaller
                    //    int handCount = packet.ReadInt();
                    //    Debug.Log(handCount);
                    //    bool right = packet.ReadBool();
                    //    Debug.Log(handCount);
                    //    int side = 0;
                    //    if (right) side = 1;
                    //    //test floatsZyy
                    //    Vector3[] handPoints1 = ReadHandPoints(packet);
                    //    //would then apply to hands but do later rn;
                    //    HandManagers[side].UpdateHands(handPoints1);
                    //    if (handCount == 2)
                    //    {
                    //        Vector3[] handPoints2 = ReadHandPoints(packet);
                    //        HandManagers[Math.Abs(side - 1)].UpdateHands(handPoints2);
                    //    }
                    //}
                    //Debug.Log(hands);
                    ProcessInput(packet);
                }
                //handle here 

                
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
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
            foreach(Action action in MainCopyThreadQueue)
            {
                action(); 
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
