using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace StellariumServer
{
    public class Connection : IDisposable
    {
        public interface IReceiveHandler
        {
            // All methods are called asynchronously. The interface implementation should
            // provide thread synchronization if necessary.
            void ReceivedGoto(double dec, double ra);
            void Error(string errText);
        };

        public Connection(IPAddress addr, int port, IReceiveHandler handler)
        {
            addr_ = addr;
            port_ = port;
            handler_ = handler;

            thread_ = new Thread(this.ThreadProc);
            thread_.Start();
        }

        public delegate void StatusChangedHandler(bool connected);
        public event StatusChangedHandler StatusChanged;
        public bool IsConnected { get { return connected_; } }

        public bool SendPosition(double dec, double ra)
        {
            lock (lockThis_)
            {
                if (sendData_ != null)
                    return false;
            }

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
            return true;
        }

        public void Close()
        {
            if (thread_ != null)
            {
                exitThread_ = true;
                evt_.Set(); // wake up thread
                evtStopped_.WaitOne();
                thread_ = null;
            }
        }

        public void Dispose()
        {
            Close();
        }


        // implementation
        private IPAddress addr_;
        private int port_;
        private bool connected_ = false;
        private IReceiveHandler handler_;
        private byte[] sendData_;
        private System.Object lockThis_ = new System.Object();
        private AutoResetEvent evt_ = new AutoResetEvent(false);
        private AutoResetEvent evtStopped_ = new AutoResetEvent(false);
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
                StatusChanged(connected);
        }

        private void ThreadProc()
        {
            if (StatusChanged != null)
                StatusChanged(false);

            TcpListener myList = new TcpListener(addr_, port_);
            myList.Start();
            while (!exitThread_)
            {
                Socket s = null;
                try
                {
                    // wait connection and connect
                    {
                        IAsyncResult connectResult = myList.BeginAcceptSocket(null, null);
                        WaitHandle[] waitObjs = new WaitHandle[] { connectResult.AsyncWaitHandle, evt_ };
                        while (!connectResult.IsCompleted && !exitThread_)
                            WaitHandle.WaitAny(waitObjs);
                        if (exitThread_)
                            break;
                        s = myList.EndAcceptSocket(connectResult);
                        SetStatusChanged(true);
                    }

                    IAsyncResult sendResult = null, receiveResult = null;
                    byte[] receivedData = new byte[20];
                    int bytesRead = 0;
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
                                if (sendData_ != null)
                                {
                                    sendData = sendData_;
                                    sendData_ = null;
                                }
                            }
                            if (sendData != null)
                                sendResult = s.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, null, null);
                        }

                        // receive
                        if (receiveResult != null && receiveResult.IsCompleted)
                        {
                            bytesRead += s.EndReceive(receiveResult);
                            receiveResult = null;

                            if (bytesRead >= receivedData.Length)
                            {
                                UInt32 uRA = receivedData[12] + (((UInt32)receivedData[13]) << 8) + (((UInt32)receivedData[14]) << 16) + (((UInt32)receivedData[15]) << 24);
                                Int32 uDec = (Int32)(receivedData[16] + (((UInt32)receivedData[17]) << 8) + (((UInt32)receivedData[18]) << 16) + (((UInt32)receivedData[19]) << 24));
                                handler_.ReceivedGoto((double)uDec * 90.0 / 0x40000000, (double)uRA * 360.0 / 0x100000000);
                                bytesRead = 0;
                            }
                        }
                        if (receiveResult == null)
                            receiveResult = s.BeginReceive(receivedData, bytesRead, receivedData.Length - bytesRead, SocketFlags.None, null, null);

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
                    handler_.Error(e.Message);
                }
                if (s != null)
                {
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                }
                SetStatusChanged(false);
            }
            myList.Stop();
            evtStopped_.Set();
        }
    }
}
