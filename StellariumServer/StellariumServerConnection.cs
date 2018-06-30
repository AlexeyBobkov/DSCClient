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
            return true;
        }

        public void Close()
        {
            if (thread_ != null)
            {
                exitThread_ = true;
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

                    // wait connection
                    while (!myList.Pending() && !exitThread_)
                        System.Threading.Thread.Sleep(1000);
                    if (exitThread_)
                        break;

                    s = myList.AcceptSocket();
                    SetStatusChanged(true);

                    IAsyncResult sent = null, receive = null;
                    byte[] rData = new byte[20];
                    int bytesRead = 0;
                    while (!exitThread_ && SocketConnected(s))
                    {
                        bool didSomething = false;
                        
                        // send
                        if (sent != null && sent.IsCompleted)
                        {
                            s.EndSend(sent);
                            sent = null;
                            didSomething = true;
                        }
                        if (sent == null)
                        {
                            byte[] sData = null;
                            lock (lockThis_)
                            {
                                if (sendData_ != null)
                                {
                                    sData = sendData_;
                                    sendData_ = null;
                                }
                            }
                            if (sData != null)
                            {
                                sent = s.BeginSend(sData, 0, sData.Length, SocketFlags.None, null, null);
                                didSomething = true;
                            }
                        }

                        // receive
                        if (receive != null && receive.IsCompleted)
                        {
                            bytesRead += s.EndReceive(receive);
                            receive = null;

                            if (bytesRead >= rData.Length)
                            {
                                UInt32 uRA = rData[12] + (((UInt32)rData[13]) << 8) + (((UInt32)rData[14]) << 16) + (((UInt32)rData[15]) << 24);
                                Int32 uDec = (Int32)(rData[16] + (((UInt32)rData[17]) << 8) + (((UInt32)rData[18]) << 16) + (((UInt32)rData[19]) << 24));
                                handler_.ReceivedGoto((double)uDec * 90.0 / 0x40000000, (double)uRA * 360.0 / 0x100000000);
                                bytesRead = 0;
                            }
                            didSomething = true;
                        }
                        if (receive == null)
                        {
                            receive = s.BeginReceive(rData, bytesRead, rData.Length - bytesRead, SocketFlags.None, null, null);
                            didSomething = true;
                        }

                        if (!didSomething && !exitThread_)
                            System.Threading.Thread.Sleep(250);
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
