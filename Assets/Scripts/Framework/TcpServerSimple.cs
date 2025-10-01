using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

public class TcpServerSimple
{
    private TcpListener listener;
    private Thread listenerThread;
    private volatile bool running = false;
    private object incomingLock = new object();
    private MyLinkedList<string> incoming = new MyLinkedList<string>();

    public void Start(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        running = true;
        listenerThread = new Thread(ListenLoop) { IsBackground = true };
        listenerThread.Start();
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
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lock (incomingLock) incoming.AddLast(line);
            }
        }
        client.Close();
    }

    public bool TryGetMessage(out string json)
    {
        lock (incomingLock)
        {
            if (incoming.Count > 0) { json = incoming.PopFirst(); return true; }
        }
        json = null;
        return false;
    }

    public void Stop()
    {
        running = false;
        listener.Stop();
    }
}