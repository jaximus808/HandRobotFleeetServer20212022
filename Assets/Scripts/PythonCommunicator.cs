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
                    float check = packet.ReadFloat();
                    Debug.Log(check);
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
