using System;
using HomeSeerAPI;

namespace HSPI_JW_VoiceEnabledDevices
{
    class HSPI : IPlugInAPI
    {
        public ManageVoiceDevices webManageVoiceDevices;
        public HomeSeerAPI.IHSApplication hs;
        public HomeSeerAPI.IAppCallbackAPI callback;
        public string IFACE_NAME = "Manage Voice Devices";
        public string managementPage = "Get_All_Voice_Devices";
        public string Instance = "";
        public static bool bShutDown = false;
        private IPlugInAPI.strInterfaceStatus interfaceStat = new IPlugInAPI.strInterfaceStatus();
        public string InitIO(string port)
        {
            interfaceStat.intStatus = IPlugInAPI.enumInterfaceStatus.OK;
            Console.WriteLine("InitIO called, plug-in is being initialized...");
            webManageVoiceDevices = new ManageVoiceDevices(this, managementPage);
            hs.RegisterPage(managementPage, IFACE_NAME, Instance);
            WebPageDesc wpd = new WebPageDesc();
            wpd.link = managementPage;
            wpd.linktext = managementPage.Replace("_", " ");
            wpd.page_title = managementPage.Replace("_", " ");
            wpd.plugInName = IFACE_NAME;
            wpd.plugInInstance = Instance;
            callback.RegisterLink(wpd);
            callback.RegisterConfigLink(wpd);
            Console.WriteLine("Plugin is initialized...");
            Console.WriteLine("-----------------------------------------");
            return "";
        }
        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            Console.WriteLine("In GetPagePlugin - HSPI");
            if (pageName == managementPage)
            {
                return (webManageVoiceDevices.GetPagePlugin(pageName, user, userRights, queryString));
            }
            else
            {
                return "This Page (" + pageName + ") Is Not Registered With The Plugin";
            }
        }
        public string PostBackProc(string page, string data, string user, int userRights)
        {
            Console.WriteLine("In PostBackProc - HSPI");
            if (page == managementPage)
            {
                return webManageVoiceDevices.postBackProc(page, data, user, userRights);
            }
            else
            {
                return "This Page (" + page + ") Is Not Registered With The Plugin";
            }
        }
        public void ShutdownIO()
        {
            Console.WriteLine("ShutdownIO called, plug-in is being Shutdown...");
            bShutDown = true;
        }
        public bool HasShutdownIO()
        {
            return bShutDown;
        }
        public HomeSeerAPI.IPlugInAPI.strInterfaceStatus InterfaceStatus()
        {
            return interfaceStat;
        }
        public string Name
        {
            get { return IFACE_NAME; }
        }
        public void HSEvent(Enums.HSEvent EventType, object[] parms)
        {
        }
        // Satisfy all the Interface requirements with defaults
        public void SetIOMulti(System.Collections.Generic.List<HomeSeerAPI.CAPI.CAPIControl> colSend)
        { 
        }
        public int Capabilities()
        {
            return (int)(HomeSeerAPI.Enums.eCapabilities.CA_IO);
        }
        public int AccessLevel()
        {
            return 1;
        }
        public bool SupportsMultipleInstances()
        {
            return false;
        }
        public bool SupportsMultipleInstancesSingleEXE()
        {
            return false;
        }
        public bool SupportsAddDevice()
        {
            return false;
        }
        public string InstanceFriendlyName()
        {
            return "";
        }
        private bool mvarActionAdvanced;
        public bool ActionAdvancedMode
        {
            get { return mvarActionAdvanced; }
            set { mvarActionAdvanced = value; }
        }
        public string ActionBuildUI(string sUnique, IPlugInAPI.strTrigActInfo ActInfo)
        {
            return "";
        }
        public bool ActionConfigured(IPlugInAPI.strTrigActInfo ActInfo)
        {
            return false;
        }
        public int ActionCount()
        {
            return 0;
        }
        public string ActionFormatUI(IPlugInAPI.strTrigActInfo ActInfo)
        {
            return "";
        }
        public string get_ActionName(int ActionNumber)
        {
            return "";
        }
        public bool HasTriggers
        {
            get { return false; }
        }
        public int TriggerCount
        {
            get { return 0; }
        }
        public string get_TriggerName(int TriggerNumber)
        {
            return "";
        }
        public int get_SubTriggerCount(int TriggerNumber)
        {
            return 0;
        }
        public string get_SubTriggerName(int TriggerNumber, int SubTriggerNumber)
        {
            return "";
        }
        public string TriggerBuildUI(string sUnique, HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
        {
            return "";
        }
        public bool get_TriggerConfigured(IPlugInAPI.strTrigActInfo TrigInfo)
        {
            return false;
        }
        public bool TriggerReferencesDevice(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo, int dvRef)
        {
            return false;
        }
        public string TriggerFormatUI(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
        {
            return "";
        }
        public bool TriggerTrue(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
        {
            return false;
        }
        public HomeSeerAPI.IPlugInAPI.strMultiReturn TriggerProcessPostUI(System.Collections.Specialized.NameValueCollection PostData, HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfoIn)
        {
            HomeSeerAPI.IPlugInAPI.strMultiReturn Ret = new HomeSeerAPI.IPlugInAPI.strMultiReturn();
            Ret.sResult = "";
            return Ret;
        }
        public bool get_Condition(IPlugInAPI.strTrigActInfo TrigInfo)
        {
            return false;
        }
        public void set_Condition(IPlugInAPI.strTrigActInfo TrigInfo, bool Value)
        {
        }
        public bool get_HasConditions(int TriggerNumber)
        {
            return false;
        }
        public bool HSCOMPort
        {
            get { return false; }
        }
        public IPlugInAPI.PollResultInfo PollDevice(int dvref)
        {
            IPlugInAPI.PollResultInfo ri = default(IPlugInAPI.PollResultInfo);
            ri.Result = IPlugInAPI.enumPollResult.OK;
            return ri;
        }
        public bool SupportsConfigDevice()
        {
            return false;
        }
        public bool SupportsConfigDeviceAll()
        {
            return false;
        }
        public string ConfigDevice(int dvRef, string user, int userRights, bool newDevice)
        {
            return "";
        }
        public Enums.ConfigDevicePostReturn ConfigDevicePost(int dvRef, string data, string user, int userRights)
        {
            return Enums.ConfigDevicePostReturn.DoneAndSave;
        }
        public HomeSeerAPI.SearchReturn[] Search(string SearchString, bool RegEx)
        {
            System.Collections.Generic.List<SearchReturn> colRET = new System.Collections.Generic.List<SearchReturn>();
            return colRET.ToArray();
        }
        public object PluginFunction(string proc, object[] parms)
        {
            return null;
        }
        public object PluginPropertyGet(string proc, object[] parms)
        {
            return null;
        }
        public void PluginPropertySet(string proc, object value)
        {
        }
        public void SpeakIn(int device, string txt, bool w, string host)
        {
        }
        public IPlugInAPI.strMultiReturn ActionProcessPostUI(System.Collections.Specialized.NameValueCollection PostData, IPlugInAPI.strTrigActInfo ActInfoIN)
        {
            HomeSeerAPI.IPlugInAPI.strMultiReturn Ret = new HomeSeerAPI.IPlugInAPI.strMultiReturn();
            Ret.sResult = "";
            return Ret;
        }
        public bool ActionReferencesDevice(IPlugInAPI.strTrigActInfo ActInfo, int dvRef)
        {
            return false;
        }
        public bool HandleAction(IPlugInAPI.strTrigActInfo ActInfo)
        {
            return true;
        }
        public bool RaisesGenericCallbacks()
        {
            return false;
        }
        public string GenPage(string link)
        {
            return null;
        }
        public string PagePut(string data)
        {
            return null;
        }
    }
}
