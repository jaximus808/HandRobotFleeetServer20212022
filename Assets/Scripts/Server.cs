using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;

public class Server
{
    public int ip {get;private set;}
    public int port {get; private set;}
    public void Start(string _ip, int _port)
    {
        port = _port;
        var endPoint = new IPEndPoint(IPAddress.Loopback, port);
        
        var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Udp);
        socket.Bind(endPoint);
        socket.Listen(128);

        _ = Task.Run( ()=> DoEcho(socket));

    }

    private async Task DoEcho(Socket socket)
    {
        do 
        {
            var clientSocket = await Task.Factory.FromAsync(
                new Func<AsyncCallback, object, IAsyncResult>(socket.BeginAccept),
                new Func<IAsyncResult, Socket>(socket.EndAccept),
                null).ConfigureAwait(false);
        }
        while(true);
    }
}
