using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System;
using System.Text;
using System.IO.Ports;

namespace _2DXTicker
{
    class Program
    {
        static readonly SerialPort serialPort = new SerialPort();
        static readonly MemoryUtilities memUtils = new MemoryUtilities();

        static readonly int _buildDate = 12142018;
        static readonly int _version =  IntPtr.Size * 8;

        static readonly string _jsonPath = (Environment.CurrentDirectory + "\\Ticker.json");
        static dynamic _jsonData;

        static bool _waitForProcess = true;

        static IntPtr _hwnd, _pHandle;
        static uint _processID;
        static Process _process;

        static string _windowTitle, _moduleName;
        static int _tickerAddress;
        static IntPtr _memoryAddress;

        static void Main()
        {
            Console.Clear();
            Console.Title = "2DXTicker";
            Console.Write("-------------------------\nCreated by GHYAKIMA#4310\n-------------------------\nBuild: {0}\nVersion: {1}x\n-------------------------\n", _buildDate,_version);

            // Parse the json file
            try { _jsonData = JObject.Parse(File.ReadAllText(_jsonPath)); }
            catch (FileNotFoundException ex) { throw ex; }

            // Wait for beatmania process
            while (_waitForProcess)
            {
                foreach (dynamic _json in _jsonData.Titles)
                {
                    // Get hwnd
                    if (_hwnd == IntPtr.Zero) { _hwnd = memUtils.FindWindow((string)_json.windowTitle); _windowTitle = (string)_json.windowTitle; }
                    else if (_hwnd != IntPtr.Zero && _windowTitle == (string)_json.windowTitle)
                    {
                        _moduleName = (string)_json.moduleName;
                        _tickerAddress = (int)_json.tickerAddress;
                    }
                }

                // Get process id 
                if (_processID == 0) { _processID = memUtils.GetWindowPID(_hwnd); }
                else
                {
                    // Get process handle 
                    if (_pHandle == IntPtr.Zero) { _pHandle = memUtils.OpenProcess(_processID); }

                    // Get base address of '_moduleName'
                    _memoryAddress = IntPtr.Add(memUtils.GetModuleAddress(_processID, _moduleName), _tickerAddress);
                    _waitForProcess = false;
                }

                /*  */
                Thread.Sleep(1000);
            }

            /* debug info  */
            if ((bool)_jsonData.Ticker.debug)
            {
                Console.Write("_windowTitle: {0}\n_moduleName: {1}\n_tickerAddress: {2}\n-------------------------", _windowTitle, _moduleName, "0x" + _tickerAddress.ToString("X"));
                Console.Write("\n_hwnd: {0}\n_processID: {1}\n_pHandle: {2}\n_memoryAddress: {3}\n-------------------------\n", _hwnd, _processID, _pHandle, "0x" + _memoryAddress.ToString("X"));
            }

            /* Create event handler */
            _process = Process.GetProcessById((int)_processID);
            _process.EnableRaisingEvents = true;
            _process.Exited += new EventHandler(process_Exited);

            /* Open serial connection */
            setupConnection();

            /* Start reading ticker bytes */
            if (serialPort.IsOpen) {
                readTickerBytes();
            }

            Thread.Sleep(-1);
        }

        private static void setupConnection()
        {
            try
            {
                serialPort.PortName = _jsonData.Ticker.COM;
                serialPort.BaudRate = _jsonData.Ticker.baud;
                serialPort.Parity = Parity.None;
                serialPort.StopBits = StopBits.One;
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;
                serialPort.Open();
            } 
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException || ex is IOException)
                {
                    throw ex;
                }
            }
        }

        private static void readTickerBytes()
        {
            /* Read 10 bytes from _memoryAddress & convert to string */
            while (!_waitForProcess && serialPort.IsOpen)
            {
                byte[] bytes = memUtils.ReadProcessMemory(_pHandle, _memoryAddress, 9, out int bytesRead);
                Console.Write("\rTicker: {0}", Encoding.ASCII.GetString(bytes).Replace("m", " ").Replace("ggggggggg", "*********"));
                serialPort.Write(Encoding.ASCII.GetString(bytes).Replace("m", " ").Replace("ggggggggg", "*********"));
                Thread.Sleep(1); 
            }               
        }

        private static void process_Exited(object sender, EventArgs e)
        {   
            // Close serialport        
            serialPort.Close();
            serialPort.Dispose();

            // Clear & return to main()
            _windowTitle = null; _moduleName = null;  _hwnd = IntPtr.Zero;
            _processID = 0;
            _pHandle = IntPtr.Zero;
            _tickerAddress = 0;  _memoryAddress = IntPtr.Zero;
            _waitForProcess = true;
            Program.Main();
        }

    }
}
