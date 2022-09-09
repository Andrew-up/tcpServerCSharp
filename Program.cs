using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using tcpServer.home;

namespace tcpServer
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static string path;

        static void Main(string[] args)
        {
       
            Program program = new Program();
            program.createLogsFolder();
            string str_directory = Environment.CurrentDirectory.ToString();
            path = Directory.GetParent(Directory.GetParent(Directory.GetParent(str_directory) + "") + "") + "";
            string value = path + "\\home\\conf.json";
            string text = File.ReadAllText(@value);
            modelServerConfig deserializedProduct = JsonConvert.DeserializeObject<modelServerConfig>(text);
            program.createServer(deserializedProduct.host, deserializedProduct.port);

        }





        void createLogsFolder()
        {
            string dirName = path + "\\logs";
            if (!Directory.Exists(dirName))
            {
                Console.WriteLine("Create directory logs");
                Directory.CreateDirectory(path + "\\logs");
            }
            else
            {
             //   Console.WriteLine(" NO CreateDirectory");
            }
        }


        void createServer(string host, int port)
        {
            TcpListener Listener = new TcpListener(IPAddress.Parse(host), port);

            try
            {
                
                Listener.Start();  // запускаем сервер
                Console.WriteLine("Сервер запущен, ожидаю подключения.");
                while (true)   // бесконечный цикл обслуживания клиентов
            {
                TcpClient client = Listener.AcceptTcpClient();  // ожидаем подключение клиента
                string clientIpv4 = IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()) + "";
                
                Thread t = new Thread(new ParameterizedThreadStart(clientConnect));
                t.Start(client);
                    

             }
            }
            catch (SocketException e)
            {
                logger.Error("Связь с клиентом IP потеряна. Причина: " + e.Message);
              
            }
            finally
            {
                if (Listener != null)
                    Listener.Stop();
            }


        }

        public void clientConnect(Object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();

            string data = null;
            Byte[] bytes = new Byte[256];
            int i;

            try
            {
                string clientIpv4 = IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()) + "";
              //  Console.WriteLine("Поток ID: ({1}) - Подключён новый клиент: IP:  {0} ", clientIpv4, Thread.CurrentThread.ManagedThreadId);
                logger.Debug("Поток ID: ({1}) - Подключён новый клиент: IP:  {0} ", clientIpv4, Thread.CurrentThread.ManagedThreadId);
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                  //  Console.WriteLine("Поток ID: ({1}) - Сообщение от клиента Ip: {2} : {0}", data, Thread.CurrentThread.ManagedThreadId, clientIpv4);
                    logger.Debug("Поток ID: ({1}) - Сообщение от клиента Ip: {2} : {0}", data, Thread.CurrentThread.ManagedThreadId, clientIpv4);
                }
                logger.Debug("Поток ID: ({1}) - Клиент IP {0} отключён ", clientIpv4, Thread.CurrentThread.ManagedThreadId);
           
                   
               // Console.WriteLine("Поток ID: ({1}) - Клиент IP {0} отключён: ", clientIpv4, Thread.CurrentThread.ManagedThreadId);
                return;

            }
            catch (SocketException e)
            {
                logger.Error("Связь с клиентом IP потеряна. Причина: " + e.Message);
             //   Console.WriteLine("Связь с клиентом IP потеряна. Причина: " + e.Message);
            }

            catch (Exception e)
            {
                logger.Error("Связь с клиентом IP потеряна. Причина: " + e.Message);
               // Console.WriteLine("Exception: {0}", e.ToString());
                client.Close();
            }
        }

    }
}
