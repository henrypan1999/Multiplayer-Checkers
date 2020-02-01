using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using UnityEngine;

public class Server : MonoBehaviour
{
    public int port = 2020;

    private List<ServerClient> clients;
    private List<Server> disconnectList;

    private TcpListener server;
    private bool serverStarted;

    public void init() {
		DontDestroyOnLoad(gameObject);
		clients = new List<ServerClient>();
		disconnectList = new List<ServerClient>();

		try {
			server = new TcpListener(IPAddress.Any, port);
			server.start();

			StartListening();
			serverStarted = true;
		}
		catch(Exception e) {
			debug.Log("Socket error: " + e.Message);
		}
    }

    private void Update(

    }

    private void StartListening() {
		server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IsAsyncResult ar) {
		TcpListener listner = (TcpListener)ar.AsyncState;

		Server sc = new ServerClient(listener.EndAcceptTcpClient(ar));
		clients.add(sc);

		StartListening();

		Debug.log("Somebody has Connected!");
    }
}

public class ServerClient
{
	public string clientName;
	public TcpClient tcp;

	public ServerClient(TcpClient tcp) {
		this.tcp = tcp;
	}
}
