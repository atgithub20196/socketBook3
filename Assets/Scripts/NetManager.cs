using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class StateObject
{
    // Client socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}
public class NetManager
{
    //定义套接字
    static Socket socket;
    //接收缓冲区
    static byte[] readBuff = new byte[1024];
    //委托类型
    public delegate void MsgListener(string str);
    //监听列表
    private static Dictionary<string, MsgListener> listeners =
        new Dictionary<string, MsgListener>();
    //消息列表
    static List<string> msgList = new List<string>();


    // ManualResetEvent instances signal completion.  
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    public static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);
    /// <summary>
    /// 添加监听
    /// </summary>
    /// <param name="msgName">消息名</param>
    /// <param name="listener">对应消息名的方法名</param>
    public static void AddListener(string msgName, MsgListener listener)
    {
        listeners[msgName] = listener;
    }
    //获取描述
    public static string GetDesc()
    {
        if (socket == null) return "";
        if (!socket.Connected) return "";
        return socket.LocalEndPoint.ToString();
    }
    /// <summary>
    /// 连接服务端
    /// </summary>
    /// <param name="ip">IP地址</param>
    /// <param name="port">进程端口号</param>
    public static void Connect(string ip, int port)
    {
        //IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
        IPAddress ipAddress = IPAddress.Parse(ip);
        //IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.BeginConnect(remoteEP, new System.AsyncCallback(ConnectCallback), socket);
        //挂起线程
        connectDone.WaitOne();
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);
            // Signal that the connection has been made.
            connectDone.Set();
            Debug.Log("Socket connected to " + client.RemoteEndPoint.ToString());
            Receive(client);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Connect fail " + ex.ToString());
        }
    }
    private static void Receive(Socket client)
    {
        try
        {
            StateObject state = new StateObject();
            state.workSocket = client;

            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
            //receiveDone.WaitOne();
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail " + ex.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            int bytesRead = client.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.
                //在数据字符串的末尾附加从服务器接收到的字符
                state.sb.Append(Encoding.Default.GetString(state.buffer, 0, bytesRead));
                // Get the rest of the data. 
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                msgList.Add(state.sb.ToString());
                state.sb.Clear();
                //receiveDone.Set();
            }
            
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }
    /// <summary>
    /// 发送信息 在哪里发送信息 就在哪里sendDone
    /// </summary>
    /// <param name="sendStr">被发送的消息字符串</param>
    public static void Send(string sendStr)
    {
        if (socket == null) return;
        if (!socket.Connected) return;
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.Default.GetBytes(sendStr);

        // Begin sending the data to the remote device.  
        socket.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), socket);
        Debug.Log(sendStr);
        //sendDone.WaitOne();
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;
            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);
            Debug.Log("Socket Sent ");
            //sendDone.Set();
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Send fail" + ex.ToString());
        }
    }


    // Update is called once per frame
    public static void Update()
    {
        if (msgList.Count <= 0)
            return;
        String msgStr = msgList[0];
        msgList.RemoveAt(0);
        string[] split = msgStr.Split('|');
        string msgName = split[0];
        string msgArgs = split[1];
        //监听回调;
        if (listeners.ContainsKey(msgName))
        {
            listeners[msgName](msgArgs);
        }
    }
}
