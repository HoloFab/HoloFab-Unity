using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
#else
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif

namespace HoloFab {
    //using UnityEngine;

    public class TCPReceive {
		#if WINDOWS_UWP
		// Connection Object References.
		private static StreamSocketListener listener;
		private static StreamSocket client;
		// Task Object Reference.
		private static CancellationTokenSource cancellationTokenSource;
		private static Task receiver;
		#else
		// Connection Object References.
		public TcpListener listener;
		public TcpClient client;
        public NetworkStream stream;
		// Thread Object Reference.
		private Thread receiver = null;
		#endif
		// Local Port
		private int localPort;
		// History:
		// - debug
		public List<string> debugMessages = new List<string>();
		// - received data
		public List<string> dataMessages = new List<string>();
		// Flag to be raised on data recepcion.
		public bool flagDataRead;

        public string status;


        public TCPReceive(int _localPort)
        {
            this.flagDataRead = true;
            this.localPort = _localPort;
            this.status = "Initiation";
        }

        // Enable connection - if not yet open.
        public void TryStartConnection(int _localPort=11111) {
            // Create a new thread to receive incoming messages.
            if (this.receiver == null)
				this.StartConnection(_localPort);
		}
		//////////////////////////////////////////////////////////////////////////
		#if WINDOWS_UWP
		private static async void StartConnection(int _localPort){
			if (TCPReceive.localPort != _localPort)
				TCPReceive.localPort = _localPort;
			// Reset.
			TCPReceive.debugMessages = new List<string>();
			TCPReceive.dataMessages = new List<string>();
			// Start the thread.
			TCPReceive.cancellationTokenSource = new CancellationTokenSource();
			TCPReceive.receiver = new Task(() => ReceiveData(), TCPReceive.cancellationTokenSource.Token);
			TCPReceive.receiver.Start();
			TCPReceive.debugMessages.Add("TCPReceive: UWP: Task Finished: " + TCPReceive.receiver.IsCompleted); // Check if even works at all.
			TCPReceive.debugMessages.Add("TCPReceive: UWP: Thread Started.");
		}
		// Disable connection.
		public static void StopConnection() {
			// Reset.
			if (TCPReceive.receiver != null) {
				TCPReceive.cancellationTokenSource.Cancel();
				TCPReceive.receiver.Wait(1);
				TCPReceive.cancellationTokenSource.Dispose();
				TCPReceive.receiver = null; // Good Practice?
				TCPReceive.debugMessages.Add("TCPReceive: UWP: Stopping Task.");
			}
			if (TCPReceive.listener != null) {
				TCPReceive.listener.Dispose();
				TCPReceive.listener = null; // Good Practice?
				TCPReceive.debugMessages.Add("TCPReceive: UWP: Stopping Listener.");
			}
		}
		// Constantly check for new messages on given port.
		private static async void ReceiveData(){
			try {
				// Open.
				TCPReceive.listener = new StreamSocketListener();
				TCPReceive.listener.ConnectionReceived += OnClientFound;
				await TCPReceive.listener.BindServiceNameAsync(TCPReceive.localPort.ToString());
			} catch (Exception exception) {
				SocketErrorStatus webErrorStatus = SocketError.GetStatus(exception.GetBaseException().HResult);
				string webError = (webErrorStatus.ToString() != "Unknown") ? webErrorStatus.ToString() :
				                                                             exception.Message;
				// Exception.
				TCPReceive.debugMessages.Add("TCPReceive: UWP: Exception: " + webError); //exception.ToString()
			}
		}
		private static async void OnClientFound(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args){
			string receiveString;
			using (StreamReader reader = new StreamReader(args.Socket.InputStream.AsStreamForRead())) {
				receiveString = await reader.ReadLineAsync();
			}
			// If buffer not empty - react to it.
			if ((!string.IsNullOrEmpty(receiveString)) && ((TCPReceive.dataMessages.Count == 0) || (TCPReceive.dataMessages[TCPReceive.dataMessages.Count-1] != receiveString))) {
				TCPReceive.debugMessages.Add("TCPReceive: UWP: New Data: " + receiveString);
				TCPReceive.dataMessages.Add(receiveString);
				TCPReceive.flagDataRead = false;
			}
		}
		//////////////////////////////////////////////////////////////////////////
		#else
		private void StartConnection(int _localPort){
            // Reset.
            this.debugMessages = new List<string>();
            this.dataMessages = new List<string>();
            // Start the thread.
            this.receiver = new Thread(new ThreadStart(this.ReceiveData));
            this.receiver.IsBackground = true;
            this.receiver.Start();
            this.debugMessages.Add("TCPReceive: Thread Started.");
		}
		// Disable connection.
		public void StopConnection() {
			// Reset.
			if (this.listener != null) {
                this.listener.Stop();
                this.debugMessages.Add("TCPReceive: Stopping Listener.");
			}
			if (this.client != null) {
                this.client.Close();
                this.debugMessages.Add("TCPReceive: Stopping Client.");
			}
            if (this.receiver != null)
            {
                this.receiver.Abort();
                this.debugMessages.Add("TCPReceive: Stopping Thread.");
            }
        }
		// Constantly check for new messages on given port.
		private void ReceiveData(){
			try {
				// Open.
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, this.localPort);
                this.listener = new TcpListener(anyIP);
                this.listener.Start();
                
				// Infinite loop.
				while (true) {
					if (this.client == null) {
                        this.status = "Listening for a Client!";
                        this.client = this.listener.AcceptTcpClient();
                        this.stream = this.client.GetStream();
                    }
                    else
                    {
                        try
                        {
                            
                            if (this.stream.DataAvailable)
                            {
                                OnClientFound();
                                this.status = "Reading Data: " + this.client.Available.ToString();
                            }
                            else
                            {
                                if (this.IsConnected)
                                {
                                    this.status = "NoData Available!";
                                }
                                else
                                {
                                    this.stream.Close();
                                    this.client.Close();
                                    this.client = null;
                                }
                            }
                            
                            
                        }
                        catch (Exception e)
                        {
                            this.stream.Close();
                            this.client.Close();
                            this.client = null;
                        }
                    }
				}
			} catch (SocketException exception) {
                // SocketException.
                this.debugMessages.Add("TCPReceive: SocketException: " + exception.ToString());
			} catch (Exception exception) {
                // Exception.
                this.debugMessages.Add("TCPReceive: Exception: " + exception.ToString());
			} finally {
                this.StopConnection();
			}
		}

		private void OnClientFound(){
			byte[] data = new byte[8096];
			string receiveString = string.Empty;
			// Receive Bytes.
			
            //int i;
            //while ((i = stream.Read(data, 0, data.Length)) != 0)
            string d = "";
            while (!d.EndsWith(";"))
            {
                int i = stream.Read(data, 0, data.Length);
                d = EncodeUtilities.DecodeData(data, 0, i);
                receiveString += d;
			}
            this.debugMessages.Add("TCPReceive: Received data: " + receiveString);

            // If buffer not empty - react to it.
            if ((!string.IsNullOrEmpty(receiveString)) && ((this.dataMessages.Count == 0) || (this.dataMessages[this.dataMessages.Count-1] != receiveString))) {
                this.dataMessages.Add(receiveString.Substring(0, receiveString.Length-1));
                this.flagDataRead = false;
			}
            this.debugMessages.Add("TCPReceive: New Data: " + receiveString);
        }

        public bool IsConnected
        {
            get
            {
                try
                {
                    if (this.client != null && this.client.Client != null && this.client.Client.Connected)
                    {
                        /* pear to the documentation on Poll:
                         * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                         * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                         * -or- true if data is available for reading; 
                         * -or- true if the connection has been closed, reset, or terminated; 
                         * otherwise, returns false
                         */

                        // Detect if client disconnected
                        if (this.client.Client.Poll(1000, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (this.client.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
#endif
    }
}