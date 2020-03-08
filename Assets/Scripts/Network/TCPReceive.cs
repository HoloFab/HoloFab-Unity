using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using UnityEngine;
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
		private StreamSocketListener listener;
		private StreamSocket client;
		// Task Object Reference.
		private CancellationTokenSource cancellationTokenSource;
		private Task receiver;
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
        
		public bool flagConnectionFound;
        
		public TCPReceive(int _localPort)
		{
			this.flagDataRead = true;
			this.localPort = _localPort;
			this.status = "Initiation";
			this.flagConnectionFound = false;
			StopConnection();
		}
        
		// Enable connection - if not yet open.
		public void TryStartConnection(int _localPort=11111) {
			// Create a new thread to receive incoming messages.
			if (this.receiver == null)
				StartConnection(_localPort);
		}
		//////////////////////////////////////////////////////////////////////////
		#if WINDOWS_UWP
		private async void StartConnection(int _localPort){
			if (this.localPort != _localPort)
				this.localPort = _localPort;
			// Reset.
			this.debugMessages = new List<string>();
			this.dataMessages = new List<string>();
			// Start the thread.
			this.cancellationTokenSource = new CancellationTokenSource();
			this.receiver = new Task(() => ReceiveConnection(), this.cancellationTokenSource.Token);
			this.receiver.Start();
			this.debugMessages.Add("TCPReceive: UWP: Task Finished: " + this.receiver.IsCompleted); // Check if even works at all.
			this.debugMessages.Add("TCPReceive: UWP: Thread Started.");
		}
		// Disable connection.
		public void StopConnection() {
			// Reset.
			if (this.receiver != null) {
				this.cancellationTokenSource.Cancel();
				this.receiver.Wait(1);
				this.cancellationTokenSource.Dispose();
				this.receiver = null; // Good Practice?
				this.debugMessages.Add("TCPReceive: UWP: Stopping Task.");
			}
			if (this.listener != null) {
				this.listener.Dispose();
				this.listener = null; // Good Practice?
				this.debugMessages.Add("TCPReceive: UWP: Stopping Listener.");
			}
		}
		// Constantly check for new messages on given port.
		private async void ReceiveConnection(){
			try {
				// Open.
				this.listener = new StreamSocketListener();
				this.listener.ConnectionReceived += OnClientFound;
				await this.listener.BindServiceNameAsync(this.localPort.ToString());
			} catch (Exception exception) {
				SocketErrorStatus webErrorStatus = SocketError.GetStatus(exception.GetBaseException().HResult);
				string webError = (webErrorStatus.ToString() != "Unknown") ? webErrorStatus.ToString() :
				                                                             exception.Message;
				// Exception.
				this.debugMessages.Add("TCPReceive: UWP: Exception: " + webError); //exception.ToString()
			}
		}
		private async void OnClientFound(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args){
			string receiveString;
			this.flagConnectionFound = true;
			this.debugMessages.Add("TCPReceive: UWP: New Client Found");
			using (StreamReader reader = new StreamReader(args.Socket.InputStream.AsStreamForRead())) {
				receiveString = await reader.ReadLineAsync();
			}
			// If buffer not empty - react to it.
			if ((!string.IsNullOrEmpty(receiveString)) && ((this.dataMessages.Count == 0) || (this.dataMessages[this.dataMessages.Count-1] != receiveString))) {
				this.debugMessages.Add("TCPReceive: UWP: New Data: " + receiveString);
				Debug.Log("TCPReceive: UWP: New Data: " + receiveString);
				this.dataMessages.Add(receiveString);
				this.flagDataRead = false;
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
			if (this.receiver != null) {
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
					} else {
						try {
                            
							if (this.stream.DataAvailable) {
								OnClientFound();
								this.status = "Reading Data: " + this.client.Available.ToString();
							} else {
								if (this.IsConnected) {
									this.status = "NoData Available!";
								} else {
									this.stream.Close();
									this.client.Close();
									this.client = null;
								}
							}
                            
                            
						} catch (Exception e) {
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
			}
			// finally {
			// 	this.StopConnection();
			// }
		}
        
		private void OnClientFound(){
			this.flagConnectionFound = true;
			byte[] data = new byte[8096];
			string receiveString = string.Empty;
			// Receive Bytes.
            
			//int i;
			//while ((i = stream.Read(data, 0, data.Length)) != 0)
			string d = "";
			while (!d.EndsWith(";")) {
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
        
		public bool IsConnected {
			get {
				try {
					if (this.client != null && this.client.Client != null && this.client.Client.Connected) {
						/* pear to the documentation on Poll:
						 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return
						 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
						 * -or- true if data is available for reading;
						 * -or- true if the connection has been closed, reset, or terminated;
						 * otherwise, returns false
						 */
                        
						// Detect if client disconnected
						if (this.client.Client.Poll(1000, SelectMode.SelectRead)) {
							byte[] buff = new byte[1];
							if (this.client.Client.Receive(buff, SocketFlags.Peek) == 0) {
								// Client disconnected
								return false;
							} else {
								return true;
							}
						}
                        
						return true;
					} else {
						return false;
					}
				} catch {
					return false;
				}
			}
		}
		#endif
	}
}