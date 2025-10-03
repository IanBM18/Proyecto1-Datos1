using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using UnityEngine;

public class TcpServerSimple
{
    private TcpListener listener;
    private Thread listenerThread;
    private volatile bool running = false;
    private object incomingLock = new object();
    private MyLinkedList<string> incoming = new MyLinkedList<string>();

    public event Action<string> OnMessageReceived;

    public void Start(int port = 5000)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        running = true;
        listenerThread = new Thread(ListenLoop) { IsBackground = true };
        listenerThread.Start();
        Debug.Log($"📡 Servidor escuchando en puerto {port}");
    }

    private void ListenLoop()
    {
        try
        {
            while (running)
            {
                var client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }
        catch (SocketException) { /* handle stop */ }
    }

    private void HandleClient(object state)
    {
        var client = (TcpClient)state;
        try
        {
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lock (incomingLock)
                    {
                        incoming.AddLast(line);
                        OnMessageReceived?.Invoke(line);
                        Debug.Log("📩 Servidor recibió: " + line);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error HandleClient: " + e.Message);
        }
        finally
        {
            client.Close();
        }
    }

    public bool TryGetMessage(out string json)
    {
        lock (incomingLock)
        {
            if (incoming.Count > 0)
            {
                json = incoming.PopFirst();
                return true;
            }
        }
        json = null;
        return false;
    }

    public void Stop()
    {
        running = false;
        listener.Stop();
        Debug.Log("❌ Servidor detenido");
    }
}
