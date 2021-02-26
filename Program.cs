using System;
using HomeSeerAPI;
using HSCF.Communication.Scs.Communication.EndPoints.Tcp;
using HSCF.Communication.ScsServices.Client;

namespace HSPI_JW_VoiceEnabledDevices
{
    class Program
    {
        private static HSPI gAppAPI;
        static HSCF.Communication.ScsServices.Client.IScsServiceClient<IAppCallbackAPI> clientCallback;
        private static HSCF.Communication.ScsServices.Client.IScsServiceClient<IHSApplication> withEventsField_client;
        private static HomeSeerAPI.IHSApplication host;
        public static void Main(string[] args)
        {
            string sIp = "127.0.0.1";
            // string sIp = "192.168.1.50";
            string instance = "";
            string sCmd = null;
            foreach (string sCmd_loopVariable in args)
            {
                sCmd = sCmd_loopVariable;
                string[] ch = new string[1];
                ch[0] = "=";
                string[] parts = sCmd.Split(ch, StringSplitOptions.None);
                switch (parts[0].ToLower())
                {
                    case "server":
                        sIp = parts[1];
                        break;
                    case "instance":
                        try
                        {
                            instance = parts[1];
                        }
                        catch (Exception)
                        {
                            instance = "";
                        }
                        break;
                }
            }
            gAppAPI = new HSPI();
            gAppAPI.Instance = instance;
            Console.WriteLine("Connecting to server at " + sIp + "...");
            client = ScsServiceClientBuilder.CreateClient<IHSApplication>(new ScsTcpEndPoint(sIp, 10400), gAppAPI);
            clientCallback = ScsServiceClientBuilder.CreateClient<IAppCallbackAPI>(new ScsTcpEndPoint(sIp, 10400), gAppAPI);
            int Attempts = 1;
        TryAgain:
            try
            {
                client.Connect();
                clientCallback.Connect();
                host = client.ServiceProxy;
                double APIVersion = host.APIVersion;
                gAppAPI.callback = clientCallback.ServiceProxy;
                gAppAPI.hs = host;
                APIVersion = gAppAPI.callback.APIVersion;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot connect attempt " + Attempts.ToString() + ": " + ex.Message);
                if (ex.Message.ToLower().Contains("timeout occurred."))
                {
                    Attempts += 1;
                    if (Attempts < 6)
                        goto TryAgain;
                }
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
                if (clientCallback != null)
                {
                    clientCallback.Dispose();
                    clientCallback = null;
                }
                wait(4);
                return;
            }
            try
            {
                host.Connect(gAppAPI.IFACE_NAME, "");
                Console.WriteLine("Connected, waiting to be initialized...");
                do
                {
                    System.Threading.Thread.Sleep(30);
                } while (client.CommunicationState == HSCF.Communication.Scs.Communication.CommunicationStates.Connected & !gAppAPI.HasShutdownIO());
                if (!gAppAPI.HasShutdownIO())
                {
                    gAppAPI.ShutdownIO();
                    Console.WriteLine("Connection lost, exiting");
                }
                else
                {
                    Console.WriteLine("Shutting down plugin");
                }
                client.Disconnect();
                clientCallback.Disconnect();
                wait(2);
                System.Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot connect(2): " + ex.Message);
                wait(2);
                System.Environment.Exit(0);
                return;
            }
        }
        public static HSCF.Communication.ScsServices.Client.IScsServiceClient<IHSApplication> client
        {
            get { return withEventsField_client; }
            set
            {
                if (withEventsField_client != null)
                {
                    withEventsField_client.Disconnected -= client_Disconnected;
                }
                withEventsField_client = value;
                if (withEventsField_client != null)
                {
                    withEventsField_client.Disconnected += client_Disconnected;
                }
            }
        }
        private static void client_Disconnected(object sender, System.EventArgs e)
        {
            Console.WriteLine("Disconnected from server - client");
        }
        private static void wait(int secs)
        {
            System.Threading.Thread.Sleep(secs * 1000);
        }
    }
}
