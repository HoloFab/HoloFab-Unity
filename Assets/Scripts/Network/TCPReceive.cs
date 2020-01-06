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
	public static class TCPReceive {
		#if WINDOWS_UWP
		// Connection Object References.
		private static StreamSocketListener listener;
		private static StreamSocket client;
		// Task Object Reference.
		private static CancellationTokenSource cancellationTokenSource;
		private static Task receiver;
		#else
		// Connection Object References.
		private static TcpListener listener;
		private static TcpClient client;
		// Thread Object Reference.
		private static Thread receiver = null;
		#endif
		// Local Port
		private static int localPort = 11111;
		// History:
		// - debug
		public static List<string> debugMessages = new List<string>();
		// - received data
		public static List<string> dataMessages = new List<string>();
		// Flag to be raised on data recepcion.
		public static bool flagDataRead = true;
        
        
		// Enable connection - if not yet open.
		public static void TryStartConnection(int _localPort=11111) {
			// Create a new thread to receive incoming messages.
			if (TCPReceive.receiver == null)
				StartConnection(_localPort);
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
			// if (TCPReceive.client != null) {
			// 	TCPReceive.client.Close();
			// 	TCPReceive.client = null; // Good Practice?
			// 	TCPReceive.debugMessages.Add("TCPReceive: Stopping Client.");
			// }
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
			} finally {
				TCPReceive.StopConnection();
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
		private static void StartConnection(int _localPort){
			if (TCPReceive.localPort != _localPort)
				TCPReceive.localPort = _localPort;
			// Reset.
			TCPReceive.debugMessages = new List<string>();
			TCPReceive.dataMessages = new List<string>();
			// Start the thread.
			TCPReceive.receiver = new Thread(new ThreadStart(ReceiveData));
			TCPReceive.receiver.IsBackground = true;
			TCPReceive.receiver.Start();
			TCPReceive.debugMessages.Add("TCPReceive: Thread Started.");
		}
		// Disable connection.
		public static void StopConnection() {
			// Reset.
			if (TCPReceive.receiver != null) {
				TCPReceive.receiver.Abort();
				TCPReceive.receiver = null; // Good Practice?
				TCPReceive.debugMessages.Add("TCPReceive: Stopping Thread.");
			}
			if (TCPReceive.listener != null) {
				TCPReceive.listener.Stop();
				TCPReceive.listener = null; // Good Practice?
				TCPReceive.debugMessages.Add("TCPReceive: Stopping Listener.");
			}
			if (TCPReceive.client != null) {
				TCPReceive.client.Close();
				TCPReceive.client = null; // Good Practice?
				TCPReceive.debugMessages.Add("TCPReceive: Stopping Client.");
			}
		}
		// Constantly check for new messages on given port.
		private static void ReceiveData(){
			try {
				// Open.
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, TCPReceive.localPort);
				TCPReceive.listener = new TcpListener(anyIP);
				TCPReceive.listener.Start();
				// Infinite loop.
				while (true) {
					TCPReceive.client = TCPReceive.listener.AcceptTcpClient();
                    
					if (TCPReceive.client.Available > 0) {
						OnClientFound();
					}
				}
			} catch (SocketException exception) {
				// SocketException.
				TCPReceive.debugMessages.Add("TCPReceive: SocketException: " + exception.ToString());
			} catch (Exception exception) {
				// Exception.
				TCPReceive.debugMessages.Add("TCPReceive: Exception: " + exception.ToString());
			} finally {
				TCPReceive.StopConnection();
			}
		}
		private static void OnClientFound(){
			byte[] data = new byte[8096];
			string receiveString = string.Empty;
			// Receive Bytes.
			NetworkStream stream = TCPReceive.client.GetStream();
			int i;
			while ((i = stream.Read(data, 0, data.Length)) != 0) {
				receiveString += EncodeUtilities.DecodeData(data, 0, i);
				TCPReceive.debugMessages.Add("TCPReceive: Received data: " + receiveString);
			}
			// If buffer not empty - react to it.
			if ((!string.IsNullOrEmpty(receiveString)) && ((TCPReceive.dataMessages.Count == 0) || (TCPReceive.dataMessages[TCPReceive.dataMessages.Count-1] != receiveString))) {
				TCPReceive.debugMessages.Add("TCPReceive: New Data: " + receiveString);
				TCPReceive.dataMessages.Add(receiveString);
				TCPReceive.flagDataRead = false;
			}
		}
		#endif
	}
}