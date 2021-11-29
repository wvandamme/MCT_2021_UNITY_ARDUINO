
using System.Threading;
using System.IO.Ports;
using System;
using UnityEngine;

namespace Arduino
{

    public class Driver
    {
        
        public enum Command
        {
            GoLeft,
            Stop,
            GoRight
        }
        
        [Serializable]
        private struct ReadData
        {
            public int command;
        }
        
        private Dispatcher _dispatcher = new Dispatcher();
        private Thread _read_thread;
        private Thread _write_thread;
        private SerialPort _port;
        private Action<Command> _command_listener = null;
        
        private object _mutex = new object();
        private bool _running = false;
        private uint _actual_position = 0;
        private uint _requested_position = 0;

        private bool Running()
        {
            lock (_mutex) {  return _running; }
        }
        
        private void Running(bool running)
        {
            lock (_mutex) {  _running = running; }
        }
        
        public Driver()
        {
        }

        public void Start()
        {
            _port = new SerialPort();
            _port.PortName = "COM11";
            _port.BaudRate = 115200;
            _port.Parity = Parity.None;
            _port.DataBits = 8;
            _port.StopBits = StopBits.One;
            _port.Handshake = Handshake.None;
            _port.ReadTimeout = 500;
            _port.WriteTimeout = 500;
            _port.Open();
            Running(true);
            _read_thread = new Thread(this.ReadTask);
            _write_thread = new Thread(this.WriteTask);
            _read_thread.Start();
            _write_thread.Start();
        }

        public void Stop()
        {
            Running(false);
            _read_thread.Join();
            _write_thread.Join();
            _port.Close();
            _read_thread = null;
            _write_thread = null;
            _port = null;
        }

        public void SetCommandListener(Action<Command> listener)
        {
            _command_listener = listener;
        }

        public void RemoveCommandListener()
        {
            _command_listener = null;
        }

        public void SendAngle(float angle_)
        {
            lock (_mutex)
            {
                _requested_position = (uint)((angle_ * 2048) / 360);
            }
        }
        
        public void Tick()
        {
            _dispatcher.Process();
        }
        
        private void ReadTask()
        {
            while (Running())
            {
                try
                {
                    ReadData data = JsonUtility.FromJson<ReadData>(_port.ReadLine());
                    if (data.command == 4)
                    {
                        _dispatcher.Dispatch(delegate
                        {
                            if (_command_listener != null) _command_listener(Command.GoLeft);
                        });
                    }
                    if (data.command == 5)
                    {
                        _dispatcher.Dispatch(delegate
                        {
                            if (_command_listener != null) _command_listener(Command.Stop);
                        });
                    }
                    if (data.command == 6)
                    {
                        _dispatcher.Dispatch(delegate
                        {
                            if (_command_listener != null) _command_listener(Command.GoRight);
                        });
                    }
                }
                catch (TimeoutException)
                {
                }
            }
        }
        
        private void WriteTask()
        {
            byte[] data = new byte[1];
            while (Running())
            {
                uint position;
                lock (_mutex)
                {
                    position = _requested_position;
                }
                if (position > _actual_position)
                {
                    _actual_position = (_actual_position + 1) % 2048;
                    if ((position - _actual_position) < 1024)
                    {
                        data[0] = 0x12;
                        _port.Write(data, 0, 1);
                    }
                    else
                    {
                        data[0] = 0x13;
                        _port.Write(data, 0, 1); 
                    }
                }
                if (position < _actual_position)
                {
                    _actual_position = _actual_position - 1;
                    if ((position - _actual_position) < 1024)
                    {
                        data[0] = 0x13;
                        _port.Write(data, 0, 1);
                    }
                    else
                    {
                        data[0] = 0x12;
                        _port.Write(data, 0, 1); 
                    }
                }
            }
        }
        
    }    
    
}
