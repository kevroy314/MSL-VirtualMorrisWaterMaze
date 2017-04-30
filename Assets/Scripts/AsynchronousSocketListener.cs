using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

// State object for reading client data asynchronously  
public class StateObject
{
    // Client  socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousSocketListener : MonoBehaviour
{
    public static bool instanceExists = false;

    public enum FrameTransmissionProtocol { Continuous, AsReply, OnRequest }

    private int incomingBufferSize = 1024;
    private int port = 5005;
    private string endToken = "<EOF>";
    private FrameTransmissionProtocol frame_protocol = FrameTransmissionProtocol.AsReply;

    private RenderTexture transmissionRenderTexture;
    private Texture2D transmissionTexture;

    private Socket listener;
    private StateObject state;
    private IPEndPoint localEndPoint;
    // Thread signal.  
    public ManualResetEvent allDone = new ManualResetEvent(false);

    public AsynchronousSocketListener() { }

    public void StartListening()
    {
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        localEndPoint = new IPEndPoint(ipAddress, port);

        // Create a TCP/IP socket.  
        listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        try
        {
            Debug.Log("Binding to local end point " + localEndPoint.ToString());
            listener.Bind(localEndPoint);
            listener.Listen(100);
        }
        catch (Exception e)
        {
            //Debug.Log(e.ToString());
        }
    }

    public void AcceptCallback(IAsyncResult ar)
    {
        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.  
        state = new StateObject();
        state.workSocket = handler;

        // Signal the main thread to continue.  
        allDone.Set();
    }

    public void ProcessRead()
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        Socket handler = state.workSocket;
        state.workSocket.Blocking = false;
        //handler.ReceiveTimeout = 10;
        // Read data from the client socket.   
        int bytesRead = 0;
        if (handler.Connected)
            bytesRead = handler.Receive(state.buffer);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.UTF8.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read   
            // more data.  
            content = state.sb.ToString();
            if (content.IndexOf(endToken) > -1)
            {
                string[] splitContent = content.Split(new string[] { endToken }, StringSplitOptions.RemoveEmptyEntries);
                if (splitContent.Length > 1)
                {
                    for (int i = 0; i < splitContent.Length - 1; i++)
                        ReceivedData(handler, splitContent[i]);
                    state.sb.Remove(0, state.sb.Length);
                    if (splitContent.Length > 1)
                        state.sb.Append(splitContent[splitContent.Length - 1]);
                }
                else
                {
                    ReceivedData(handler, splitContent[0]);
                }
            }
        }
    }

    private void ReceivedData(Socket handler, string content)
    {
        // All the data has been read from the   
        // client. Display it on the console.  
        //Debug.Log("Read " + content.Length + " bytes from socket. \n Data : " + content);
        if (frame_protocol == FrameTransmissionProtocol.AsReply)
        {
            //Debug.Log("Frame Protocol is AsReply, Sending Reply Frame");
            // Echo the data back to the client.  
            SendFrame(handler);
        }
        else if(frame_protocol == FrameTransmissionProtocol.OnRequest)
        {
            Message m = new Message(content);
            if(m.Type == "ImageRequest")
            {
                //Debug.Log("Frame Protocol is OnRequest and ImageRequest type received, Sending Reply Frame");
                // Echo the data back to the client.  
                SendFrame(handler);
            }
        }
    }

    public void SendFrame(Socket handler)
    {
        byte[] typeData = Encoding.UTF8.GetBytes("Image");
        byte[] EOF = Encoding.UTF8.GetBytes(endToken);
        Send(handler, typeData);
        //Send(handler, Encoding.UTF8.GetBytes("ThisIsATestString"));
        Send(handler, GetFrameData());
        Send(handler, EOF);
    }

    public class Message
    {
        private string type;
        private System.Object value;

        public Message()
        {
            Type = null;
            Value = null;
        }

        public Message(string data)
        {
            if(data.StartsWith("ImageRequest"))
            {
                Type = "ImageRequest";
                Value = null;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public object Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }
    }

    private byte[] GetFrameData()
    {
        //If the textures are uninitialized or the wrong size, make them and/or destroy and make them
        if(transmissionRenderTexture == null)
            transmissionRenderTexture = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 1);
        if(transmissionTexture == null)
            transmissionTexture = new Texture2D(Camera.main.pixelWidth, Camera.main.pixelHeight);
        if(transmissionRenderTexture.width != Camera.main.pixelWidth || transmissionRenderTexture.height != Camera.main.pixelHeight)
        {
            Destroy(transmissionRenderTexture);
            transmissionRenderTexture = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 1);
        }
        if(transmissionTexture.width != Camera.main.pixelWidth || transmissionTexture.height != Camera.main.pixelHeight)
        {
            Destroy(transmissionTexture);
            transmissionTexture = new Texture2D(Camera.main.pixelWidth, Camera.main.pixelHeight);
        }
        //Get the camera image
        Camera.main.targetTexture = transmissionRenderTexture;
        Camera.main.Render();
        RenderTexture.active = transmissionRenderTexture;
        transmissionTexture.ReadPixels(new Rect(0, 0, transmissionTexture.width, transmissionTexture.height), 0, 0, false);
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        byte[] bytes = transmissionTexture.EncodeToJPG();
        return bytes;
    }

    private void Send(Socket handler, byte[] data)
    {
        handler.SendTimeout = 10;
        // Convert the string data to byte data using ASCII encoding.  
        handler.Send(data);
    }

    void Awake()
    {
        if (instanceExists)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(transform.gameObject);
            instanceExists = true;
            Debug.Log("Starting TCP Server...");
            StartListening();
            currentState = State.Reset;
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application quitting. Shutting down socket.");
        try
        {
            state.workSocket.Shutdown(SocketShutdown.Both);
            state.workSocket.Close();
        }
        catch (SocketException) { }
    }

    private bool firstUpdate = true;
    public enum State { Waiting, Reset, Connected };
    private State currentState;
    void Update()
    { 
        // Bind the socket to the local endpoint and listen for incoming connections.  
        try
        {
            if (currentState == State.Reset) {
                Debug.Log("Entering reset state.");
                // Set the event to nonsignaled state.  
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                Debug.Log("Waiting for a connection...");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);
                currentState = State.Waiting;
            }
            if(currentState == State.Waiting) {
                // Wait until a connection is made before continuing.  
                if (allDone.WaitOne(10))
                    currentState = State.Connected;
            }
            if (currentState == State.Connected)
            {
                if (!IsConnected(listener))
                {
                    Debug.Log("Socket disconnected. Resetting...");
                    currentState = State.Reset;
                }
                else
                {
                    ProcessRead();
                    if (frame_protocol == FrameTransmissionProtocol.Continuous)
                        SendFrame(state.workSocket);
                }
            }
        }
        catch (Exception e)
        {
            //Debug.Log(e.ToString());
        }
    }
    public bool IsConnected(Socket socket)
    {
        try
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }
        catch (SocketException) { return false; }
    }
}