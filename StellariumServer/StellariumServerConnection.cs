using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace StellariumServer
{
    public class Connection : IDisposable
    {
        public Connection(IPAddress addr, int port)
        {
            myList_ = new TcpListener(addr, port);
            myList_.Start();

            thread_ = new Thread(this.ThreadProc);
            thread_.Start();
        }

        // connection status
        public bool IsConnected { get { return connected_; } }

        // connection status changed event
        public delegate void StatusChangedHandler();
        public event StatusChangedHandler StatusChanged;

        // error event
        public delegate void ErrorHandler(string errText);
        public event ErrorHandler Error;

        // GoTo received from Stellarium event
        public delegate void ReceivedGotoHandler(double dec, double ra);
        public event ReceivedGotoHandler ReceivedGoto;

        // Send position to Stellarium
        public void SendPosition(double dec, double ra)
        {
            UInt32 uRA = (UInt32)(ra * 0x100000000 / 360.0);
            Int32 iDec = (Int32)(dec * 0x40000000 / 90.0);
            
            byte[] data = new byte[24];
            data[0] = 24;
            data[12] = (byte)uRA;
            data[13] = (byte)(uRA >> 8);
            data[14] = (byte)(uRA >> 16);
            data[15] = (byte)(uRA >> 24);
            data[16] = (byte)iDec;
            data[17] = (byte)(iDec >> 8);
            data[18] = (byte)(iDec >> 16);
            data[19] = (byte)(iDec >> 24);

            lock (lockThis_)
            {
                sendData_ = data;
            }
            evt_.Set(); // wake up thread
            return;
        }

        public void Close()
        {
            Thread thread = null;
            lock (lockThis_)
            {
                thread = thread_;
                thread_ = null;
            }
            if (thread != null)
            {
                exitThread_ = true;
                evt_.Set(); // wake up thread
                thread.Join();
            }
            TcpListener myList = null;
            lock (lockThis_)
            {
                myList = myList_;
                myList_ = null;
            }
            if (myList != null)
                myList.Stop();
        }

        public void Dispose()
        {
            Close();
        }


        // implementation
        private TcpListener myList_;
        private bool connected_ = false;
        private byte[] sendData_;
        private System.Object lockThis_ = new System.Object();
        private AutoResetEvent evt_ = new AutoResetEvent(false);
        private bool exitThread_ = false;
        private Thread thread_;

        private static bool SocketConnected(Socket s)
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }

        private void SetStatusChanged(bool connected)
        {
            connected_ = connected;
            if (StatusChanged != null)
                StatusChanged();
        }

        private void SendReceivedGoto(double dec, double ra)
        {
            if (ReceivedGoto != null)
                ReceivedGoto(dec, ra);
        }

        private void SendError(string errText)
        {
            if (Error != null)
                Error(errText);
        }

        private void ThreadProc()
        {
            SetStatusChanged(false);
            while (!exitThread_)
            {
                Socket s = null;
                try
                {
                    // wait connection and connect
                    {
                        IAsyncResult connectResult = myList_.BeginAcceptSocket(null, null);
                        WaitHandle[] waitObjs = new WaitHandle[] { connectResult.AsyncWaitHandle, evt_ };
                        while (!connectResult.IsCompleted && !exitThread_)
                            WaitHandle.WaitAny(waitObjs);
                        if (exitThread_)
                            break;
                        s = myList_.EndAcceptSocket(connectResult);
                        SetStatusChanged(true);
                    }

                    IAsyncResult sendResult = null, receiveResult = null;
                    byte[] receivedData = new byte[20];
                    int bytesReceived = 0;
                    while (!exitThread_ && SocketConnected(s))
                    {
                        // send
                        if (sendResult != null && sendResult.IsCompleted)
                        {
                            s.EndSend(sendResult);
                            sendResult = null;
                        }
                        if (sendResult == null)
                        {
                            byte[] sendData = null;
                            lock (lockThis_)
                            {
                                sendData = sendData_;
                                sendData_ = null;
                            }
                            if (sendData != null)
                                sendResult = s.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, null);
                        }

                        // receive
                        if (receiveResult != null && receiveResult.IsCompleted)
                        {
                            bytesReceived += s.EndReceive(receiveResult);
                            receiveResult = null;

                            if (bytesReceived >= receivedData.Length)
                            {
                                UInt32 uRA = receivedData[12] + (((UInt32)receivedData[13]) << 8) + (((UInt32)receivedData[14]) << 16) + (((UInt32)receivedData[15]) << 24);
                                Int32 uDec = (Int32)(receivedData[16] + (((UInt32)receivedData[17]) << 8) + (((UInt32)receivedData[18]) << 16) + (((UInt32)receivedData[19]) << 24));
                                SendReceivedGoto((double)uDec * 90.0 / 0x40000000, (double)uRA * 360.0 / 0x100000000);
                                bytesReceived = 0;
                            }
                        }
                        if (receiveResult == null)
                            receiveResult = s.BeginReceive(receivedData, bytesReceived, receivedData.Length - bytesReceived, SocketFlags.None, null, null);

                        if (exitThread_)
                            break;

                        // wait sent, receive + external event
                        WaitHandle.WaitAny(sendResult != null ?
                            new WaitHandle[] { receiveResult.AsyncWaitHandle, sendResult.AsyncWaitHandle, evt_ } :
                            new WaitHandle[] { receiveResult.AsyncWaitHandle, evt_ });
                    }
                }
                catch (Exception e)
                {
                    SendError(e.Message);
                }
                if (s != null)
                {
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                }
                SetStatusChanged(false);
            }
        }
    }
}
