using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HoloFab {
	public static class TCPReceive {
		// Connection Object Reference.
		private static TcpListener listener;
		private static TcpClient client;
		// Thread Object Reference.
		private static Thread receiveThread = null;
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
			if (TCPReceive.receiveThread == null) {
				if (TCPReceive.localPort != _localPort)
					TCPReceive.localPort = _localPort;
				// Reset.
				TCPReceive.debugMessages = new List<string>();
				TCPReceive.dataMessages = new List<string>();
				// Start the thread.
				TCPReceive.receiveThread = new Thread(new ThreadStart(ReceiveData));
				TCPReceive.receiveThread.IsBackground = true;
				TCPReceive.receiveThread.Start();
				TCPReceive.debugMessages.Add("TCPReceive: Thread Started.");
			}
		}
		// Disable connection.
		public static void StopConnection() {
			// Reset.
			if (TCPReceive.receiveThread != null) {
				TCPReceive.receiveThread.Abort();
				TCPReceive.receiveThread = null; // Good Practice?
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
		private static void ReceiveData() {
			// Open.
			IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, TCPReceive.localPort);
			TCPReceive.listener = new TcpListener(anyIP);
			TCPReceive.listener.Start();
			// Infinite loop.
			try {
				while (true) {
					TCPReceive.client = TCPReceive.listener.AcceptTcpClient();
                    
					if (TCPReceive.client.Available > 0) {
                        byte[] data = new byte[8096];
                        string receiveString = string.Empty;
                        // Receive Bytes.
                        NetworkStream stream = TCPReceive.client.GetStream();
						int i;
						while ((i = stream.Read(data, 0, data.Length)) != 0) {
							receiveString += EncodeUtilities.DecodeData(data, 0, i);
                            TCPReceive.debugMessages.Add("TCPReceive: Receiveddata: " + receiveString);
						}
						// If buffer not empty - react to it.
						if ((!string.IsNullOrEmpty(receiveString)) && ((TCPReceive.dataMessages.Count == 0) || (TCPReceive.dataMessages[TCPReceive.dataMessages.Count-1] != receiveString))) {
							TCPReceive.debugMessages.Add("TCPReceive: New Data: " + receiveString);
							TCPReceive.dataMessages.Add(receiveString);
							TCPReceive.flagDataRead = false;
						}
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
	}
}