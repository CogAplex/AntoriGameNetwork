﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Linq;
using System.Reflection;

class SessionServer : Singleton<SessionServer>, IRunServer {
    int session_id = 1;
    const int PORT = 8001;
    Dictionary<int, TcpClientHandler> connectedTcpClientPool = new Dictionary<int, TcpClientHandler>();
    
    public void Start() {
        TcpListener listener = new TcpListener(IPAddress.Any, PORT);

        try {
            listener.Start();
            listener.BeginAcceptTcpClient(OnAccept, listener);
        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("Session Server Start");
    }

    void OnAccept(IAsyncResult ar) {
        Console.WriteLine("Client Connected");
        TcpListener listener = (TcpListener)ar.AsyncState;
        TcpClient tcpClient = listener.EndAcceptTcpClient(ar);

        TcpClientHandler handler = new TcpClientHandler(tcpClient, session_id, this);

        if(connectedTcpClientPool.TryAdd(session_id, handler)) {
            session_id = Interlocked.Increment(ref session_id);
        }

        listener.BeginAcceptTcpClient(OnAccept, listener);
    }

    public void SendPacketAll(IProtocol protocol) {
        foreach(var handler in connectedTcpClientPool.Values) {
            handler.SendPacket(protocol);
        }
    }

    public void OnClientLeave(int session_id) {
        connectedTcpClientPool.Remove(session_id);
    }
}
