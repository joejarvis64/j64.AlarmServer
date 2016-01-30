using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace j64.AlarmServer
{
    /// <summary>
    /// All communication with the TPI is handled via this class.  It has the ability to send commands
    /// over the socket, monitor for responses being received from the TPI, and will also send a poll
    /// command to the TPI periodically so the connectino will not time out.
    /// </summary>
    public class TPISocket
    {
        #region Public properties
        /// <summary>
        /// The hostname or IP address of the envisalink device
        /// </summary>
        public string Host { get; set; } = "envisalink";

        /// <summary>
        /// THe TCP port the TPI app is listening on
        /// </summary>
        public int Port { get; set; } = 4025;

        /// <summary>
        /// The number of seconds between poll commands that are sent to the TPI
        /// </summary>
        public int PollInterval { get; set; } = 1200;

        /// <summary>
        /// Indicates whether the socket is currently connected to the TPI
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (_client == null || _client.Connected == false)
                    return false;
                else
                    return true;
            }
        }
        #endregion

        #region Private variables
        private TcpClient _client = null;
        private Thread _listenThread = null;
        private Thread _pollThread = null;
        private Object thisLock = new Object();
        private bool _shutdown = false;
        #endregion

        #region Public Events
        /// <summary>
        /// Fired when a response is received from the TPI
        /// </summary>
        public event EventHandler<TpiResponse> ResponseReceived;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public TPISocket()
        {
        }
        #endregion

        #region Send Data to TPI
        /// <summary>
        /// Send a command to the TPI.  This method will issue a lock to prevent multiple threads
        /// from sending a command at the same time.
        /// </summary>
        /// <param name="tpiRequest"></param>
        public void ExecuteCommand(TpiCommand tpiRequest)
        {
            // Take a lock so only one request can be sent at a time
            lock (thisLock)
            {
                if (_client == null || _client.Connected == false)
                {
                    MyLogger.LogError($"Socket is not connected.  Cannot send command => {((int)tpiRequest.Command):D3}:{tpiRequest.Command.ToString().PadRight(20)} - {tpiRequest.CommandData}");
                    return;
                }

                MyLogger.LogInfo($"> {((int)tpiRequest.Command):D3}:{tpiRequest.Command.ToString().PadRight(20)} - {tpiRequest.CommandData}");

                // Send the request
                var data = Encoding.ASCII.GetBytes(tpiRequest.TPIData);
                var serverStream = _client.GetStream();
                serverStream.Write(data, 0, data.Length);
                serverStream.Flush();
            }
        }
        #endregion

        #region Connect/Disconnect methods
        /// <summary>
        /// Begin a connection with the TPI and start listening for responses.
        /// </summary>
        public void StartSocket()
        {
            _listenThread = new Thread(new ThreadStart(ListenForData));
            _listenThread.Name = "TPI_Listener";
            _listenThread.Start();

            _pollThread = new Thread(new ThreadStart(PollTpi));
            _pollThread.Name = "TPI_Poll";
            _pollThread.Start();
        }

        public void Disconnect()
        {
            _shutdown = true;
            _client.Client.Shutdown(SocketShutdown.Both);
            _client.Client.Dispose();
        }

        private void EnsureConnection()
        {
            do
            {
                lock (thisLock)
                {
                    try
                    {
                        // Make sure we have not already connnected on another thread
                        if (_client != null && _client.Connected)
                            return;

                        // Clean up anything that is outstanding
                        if (_client != null)
                        {
                            _client.Client.Shutdown(SocketShutdown.Both);
                            _client.Dispose();
                        }

                        MyLogger.LogInfo("Reconnecting the socket");
                        _client = new TcpClient();
                        Task ca = _client.ConnectAsync(Host, Port);
                        if (ca.Wait(15000) == false)
                        {
                            MyLogger.LogError($"ERROR: Could not connect within 15 seconds to {Host} on Port {Port}");
                        }

                        // Return if we connected properly
                        if (_client.Connected)
                            return;

                        _client.Dispose();
                        _client = null;
                        MyLogger.LogError($"ERROR: Could not connect to {Host} on Port {Port}");
                    }
                    catch (Exception ex)
                    {
                        MyLogger.LogError($"ERROR: trying to connect {Host} on Port {Port} - {MyLogger.ExMsg(ex)}");
                        _client = null;
                    }

                    // Wait 5 seconds before trying to re-connect
                    MyLogger.LogInfo("Waiting 5 seconds before trying to re-connect");
                    Thread.Sleep(5000);
                }

            } while (_client == null);
        }
        #endregion

        #region Listen & Poll methods for the TPI Connection
        /// <summary>
        /// Listen for data coming from the TPI
        /// </summary>
        private void ListenForData()
        {
            // Listen forever 
            while (true)
            {
                EnsureConnection();

                string[] lines;
                NetworkStream clientStream = null;
                try
                {
                    clientStream = _client.GetStream();
                    var message = new byte[4096];

                    while (!_shutdown)
                    {
                        var bytesRead = 0;

                        try
                        {
                            //blocks until a client sends a message
                            bytesRead = clientStream.Read(message, 0, 4096);
                        }
                        catch (Exception ex)
                        {
                            //a socket error has occured
                            MyLogger.LogError($"ListenData Exception: {MyLogger.ExMsg(ex)}");

                            // Wait 5 seconds before trying to re-connect
                            MyLogger.LogInfo("Waiting 5 seconds before trying to re-connect");
                            Thread.Sleep(5000);

                            break;
                        }

                        if (bytesRead == 0)
                        {
                            //the client has disconnected from the server
                            break;
                        }

                        //message has successfully been received
                        var encoder = new ASCIIEncoding();


                        var value = encoder.GetString(message, 0, bytesRead);
                        lines = Regex.Split(value, "\r\n");
                        foreach (string line in lines)
                        {
                            if (!String.IsNullOrEmpty(line))
                            {
                                var r = new TpiResponse(line);

                                MyLogger.LogInfo($"< {((int)r.Command):D3}:{r.Command.ToString().PadRight(20)} - {r.Data}");

                                if (ResponseReceived != null)
                                    ResponseReceived(this, r);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MyLogger.LogError(MyLogger.ExMsg(ex));
                }
            }
        }

        /// <summary>
        /// Listen for data coming from the TPI
        /// </summary>
        private void PollTpi()
        {
            // Start the first poll 10 seconds after start up
            Thread.Sleep(10 * 1000);

            //PollInterval
            while (true)
            {
                EnsureConnection();

                try
                {
                    TpiCommand req = new TpiCommand(RequestCommand.Poll);
                    ExecuteCommand(req);

                    req = new TpiCommand(RequestCommand.KeepAlive, "1");
                    ExecuteCommand(req);

                    // Dump the Zone Timers during the poll just to keep them relatively fresh!
                    //req = new TpiCommand(RequestCommand.DumpZoneTimers);
                    //ExecuteCommand(req);
                }
                catch (Exception ex)
                {
                    MyLogger.LogError($"Could not execute polling command {MyLogger.ExMsg(ex)}");
                }

                Thread.Sleep(PollInterval * 1000);
            }
        }
        #endregion
    }
}
