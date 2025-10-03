using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpClientSimple
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    public event Action<string> OnMessageReceived;

    public void Connect(string host = "127.0.0.1", int port = 5000)
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();

            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            receiveThread.Start();

            Debug.Log($"🔗 Cliente conectado al servidor {host}:{port}");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error al conectar cliente: " + e.Message);
        }
    }

    private void ReceiveLoop()
    {
        try
        {
            byte[] buffer = new byte[1024];
            while (client != null && client.Connected)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    OnMessageReceived?.Invoke(msg);
                    Debug.Log("📩 Cliente recibió: " + msg);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error en ReceiveLoop: " + e.Message);
        }
    }

    public void Send(string message)
    {
        try
        {
            if (client != null && client.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Debug.Log("📤 Cliente envió: " + message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error al enviar: " + e.Message);
        }
    }

    public void Disconnect()
    {
        receiveThread?.Abort();
        stream?.Close();
        client?.Close();
        Debug.Log("❌ Cliente desconectado");
    }
}
