using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

public class PythonCommunicator : MonoBehaviour
{
    // Start is called before the first frame update

    //going to need a python communicator object reference 
    public HandRendererOne[] HandManagers; 

    public bool active = false; 

    public int inPort = 8000; //port to receive data
    public int outPort = 8001; //port to send data

    private UdpClient client; 
    private IPEndPoint remoteEndPoint; 
    private Thread receiveThread;

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

        client = new UdpClient(inPort);

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true; 
        receiveThread.Start();
        Debug.Log("Listening");

    }

    private Vector3[] ReadHandPoints(Packet packet)
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
                bool hands = packet.ReadBool();
                if(hands)
                {
                    //can make this smaller
                    int handCount = packet.ReadInt();
                    Debug.Log(handCount);
                    bool right = packet.ReadBool();
                    Debug.Log(handCount);
                    int side = 0;
                    if(right) side = 1;
                    //test floatsZyy
                    Vector3[] handPoints1 = ReadHandPoints(packet);
                    //would then apply to hands but do later rn;
                    HandManagers[side].UpdateHands(handPoints1);
                    if(handCount == 2) 
                    {
                        Vector3[] handPoints2 = ReadHandPoints(packet);
                        HandManagers[Math.Abs(side-1)].UpdateHands(handPoints2);
                    }
                }
                Debug.Log(hands);
                ProcessInput(packet);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
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

    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }
}
