using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;

namespace SerialPortSupport
{
    public class SerialConnection : IDisposable
    {
        public static string[] MakeCorrectPortNames(string[] ports)
        {
            string[] ret = new string[ports.Length];
            Regex comRegex = new Regex(@"^(?<port>COM[0-9]+).?", RegexOptions.IgnoreCase);
            for (int i = 0; i < ports.Length; ++i)
            {
                string s = ports[i];
                try
                {
                    string portName = comRegex.Match(s).Result("${port}");
                    ret[i] = (portName.Length > 0) ? portName : s;
                }
                catch
                {
                    ret[i] = s;
                }
            }
            return ret;
        }

        public interface IReceiveHandler
        {
            // All methods are called asynchronously. The interface implementation should
            // provide thread synchronization if necessary.
            void Received(byte[] data);
            void Error();
        };

        public SerialConnection(string portName, int baudRate)
            : this(portName, baudRate, Parity.None, 8, StopBits.One, Handshake.None, 1000, 1000)
        {
        }

        public SerialConnection(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake, int readTimeout, int writeTimeout)
        {
            portName_ = portName;
            baudRate_ = baudRate;
            port_ = new SerialPort();
            port_.PortName = portName;
            port_.BaudRate = baudRate;
            port_.Parity = parity;
            port_.DataBits = dataBits;
            port_.StopBits = stopBits;
            port_.Handshake = handshake;
            port_.ReadTimeout = readTimeout;
            port_.WriteTimeout = writeTimeout;
            port_.Open();

            evt_ = new AutoResetEvent(false);
            evtStopped_ = new AutoResetEvent(false);
            thread_ = new Thread(this.ThreadProc);
            thread_.Start();
        }

        public string PortName
        {
            get { return portName_; }
        }

        public int BaudRate
        {
            get { return baudRate_; }
        }

        public void SendReceiveRequest(byte[] sendData, int receiveCnt, IReceiveHandler receiveHandler)
        {
            if (receiveCnt > 0 && receiveHandler != null)
            {
                lock (lockThis_)
                {
                    requests_.Enqueue(new ReceiveRequest(sendData, receiveCnt, receiveHandler));
                }
                evt_.Set(); // thread, wake up
            }
        }

        public void Close()
        {
            if (thread_ != null)
            {
                exitThread_ = true;
                evt_.Set();
                evtStopped_.WaitOne();
                thread_ = null;
                try
                {
                    port_.Close();
                }
                catch (Exception)
                {
                }
                port_.Dispose();
            }
        }

        public void Dispose()
        {
            Close();
        }

        // implementation
        private struct ReceiveRequest
        {
            public byte[] sendData_;
            public int cnt_;
            public IReceiveHandler handler_;
            public ReceiveRequest(byte[] sendData, int cnt, IReceiveHandler h)
            {
                sendData_ = sendData;
                cnt_ = cnt;
                handler_ = h;
            }
        }

        private string portName_;
        private int baudRate_;
        private SerialPort port_;
        private System.Object lockThis_ = new System.Object();
        private Queue<ReceiveRequest> requests_ = new Queue<ReceiveRequest>();
        private AutoResetEvent evt_, evtStopped_;
        private bool exitThread_ = false;
        private Thread thread_;

        private void ThreadProc()
        {
            for (; ; )
            {
                evt_.WaitOne();
                if (exitThread_)
                    break;
                for (; ; )
                {
                    ReceiveRequest request = new ReceiveRequest(null, 0, null);
                    bool requestValid;
                    lock (lockThis_)
                    {
                        if (requests_.Count > 0)
                        {
                            request = requests_.Dequeue();
                            requestValid = true;
                        }
                        else
                            requestValid = false;
                    }
                    if (!requestValid)
                        break;

                    try
                    {
                        // send data
                        port_.Write(request.sendData_, 0, request.sendData_.Length);

                        byte[] data = new byte[request.cnt_];
                        int cnt = 0;
                        while (cnt < request.cnt_)
                            cnt += port_.Read(data, cnt, data.Length - cnt);
                        request.handler_.Received(data);
                    }
                    catch (Exception)
                    {
                        lock (lockThis_)
                        {
                            requests_.Clear();
                        }
                        request.handler_.Error();
                        break;
                    }
                }
                if (exitThread_)
                    break;
            }
            lock (lockThis_)
            {
                requests_.Clear();
            }
            evtStopped_.Set();
        }
    }
}
