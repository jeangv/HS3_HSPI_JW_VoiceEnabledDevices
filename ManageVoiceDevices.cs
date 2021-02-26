using System;
using Scheduler;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Linq;

namespace HSPI_JW_VoiceEnabledDevices
{
    class ManageVoiceDevices : PageBuilderAndMenu.clsPageBuilder
    {
        private HSPI hspi;
        private HomeSeerAPI.IHSApplication hs;
        public string INIFileName = "Valid_Voice_Enabled_Devices.ini";
        public string HomeSeerIPAddress;
        public string HomeSeerWebServerPort;
        public List<int> ListOfAllCurrentHSVoiceEnabledRefs = new List<int>();
        public List<int> ListOfINIApprovedRefs = new List<int>();
        public List<int> ListOfUnapprovedVoiceRefs = new List<int>();
        public List<int> ListOfValidDevicesInINIButNotVoiceInHSRefs = new List<int>();
        public List<int> ListOfApproveVoiceInINIandAlsoVoiceInHSRefs = new List<int>();
        public Dictionary<int, string> DictionaryOfRefToFullDeviceName = new Dictionary<int, string>();
        public Dictionary<int, string> DictionaryOfRefToVoiceCommand = new Dictionary<int, string>();
        public List<int> ListOfChecked = new List<int>();
        public List<int> ListOfUnChecked = new List<int>();
        public ManageVoiceDevices(HSPI hspi, string pageName) : base(pageName)
        {
            this.hspi = hspi;
            hs = hspi.hs;
            Console.WriteLine("In Constructor - ManageVoiceDevices");
            HomeSeerIPAddress = hs.GetIPAddress();
            HomeSeerWebServerPort = hs.WebServerPort().ToString();
        }
        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            Console.WriteLine("In GetPagePlugin - ManageVoiceDevices");
            StringBuilder stb = new StringBuilder();
            PopulateAllLists();
            this.reset();
            this.AddHeader(hs.GetPageHeader(pageName, pageName.Replace("_", " "), "", "", false, false));
            stb.Append($"<table width='1000' border='0'><tr><td align='center'><h1>{ pageName.Replace("_", " ") }</h1></td></tr><tr><td align='center'><h2>HomeSeer Version: { hs.Version() }<br />HomeSeer Uptime: { hs.SystemUpTime() }</h2><hr /></td></tr>");
            stb.Append(Populate_Voice_Entries_In_Page_Section(pageName, ListOfUnapprovedVoiceRefs, "UnapprovedHSVoiceEnabled", "List of Unapproved Voice Enabled HomeSeer Devices", "Voice Enabled HomeSeer Device Will Be Added To The Approved List Of Voice Devices", "Voice Enablement Will Be Removed From The HomeSeer Device"));
            stb.Append(Populate_Voice_Entries_In_Page_Section(pageName, ListOfValidDevicesInINIButNotVoiceInHSRefs, "NotVoiceEnabledInHSButAreInINI", "List Of Voice Approved Devices In Config File That Are Not Voice Enabled In HomeSeer", "HomeSeer Device Will Have Voice Enablement Activated", "Device Will Be Removed From The Approved List Of Voice Devices"));
            stb.Append(Populate_Voice_Entries_In_Page_Section(pageName, ListOfApproveVoiceInINIandAlsoVoiceInHSRefs, "BothApprovedAndInHS", "List Of Voice Approved Devices That Are Also Voice Enabled In HomeSeer", "Device Will Stay Both Voice Enabled In HomeSeer And On The Approved List Of Voice Devices", "Voice Enablement Will Be Removed And Device Will Be Removed From Approved List Of Voice Devices"));
            stb.Append("</table>");
            stb.Append("<script>");
            stb.Append("    function update_hidden_field(checkbox_element) {");
            stb.Append("	    if (checkbox_element.checked) {");
            stb.Append("		    document.getElementById('hcb_' + checkbox_element.name.replace('vrr_', '')).disabled = true;");
            stb.Append("    	} else {");
            stb.Append("    		document.getElementById('hcb_' + checkbox_element.name.replace('vrr_', '')).disabled = false;");
            stb.Append("    	}");
            stb.Append("    }");
            stb.Append("	function Toggle_Button_Value(Button_Element) {");
            stb.Append("		if (Button_Element.value == 'UnCheck All') {");
            stb.Append("			Button_Element.value = 'Check All';");
            stb.Append("    		$('.' + Button_Element.name + ' input:checkbox').prop('checked', false).trigger('change');");
            stb.Append("		} else if (Button_Element.value == 'Check All') {");
            stb.Append("  			Button_Element.value = 'UnCheck All';");
            stb.Append("    		$('.' + Button_Element.name + ' input:checkbox').prop('checked', true).trigger('change');");
            stb.Append("		}");
            stb.Append("	}");
            stb.Append("</script>");
            this.AddBody(stb.ToString());
            this.AddFooter(hs.GetPageFooter());
            this.suppressDefaultFooter = true;
            return this.BuildPage();
        }
        public override string postBackProc(string page, string data, string user, int userRights)
        {
            Console.WriteLine("In postBackProc - ManageVoiceDevices");
            System.Collections.Specialized.NameValueCollection parts;
            parts = HttpUtility.ParseQueryString(data);
            string Type_Of_Command = "";
            ListOfChecked.Clear();
            ListOfUnChecked.Clear();
            foreach (string part in parts)
            {
                if (part.StartsWith("vct_"))
                    Check_And_Modify_Voice_Command_As_Needed(part, parts[part]);
                else if (part.StartsWith("vrr_"))
                {
                    int Reference = System.Convert.ToInt32(part.Replace("vrr_", ""));
                    if (parts[part] == "checked")
                        ListOfChecked.Add(Reference);
                    else if (parts[part] == "unchecked")
                        ListOfUnChecked.Add(Reference);
                }
                else if (part == "id")
                    Type_Of_Command = parts[part];
            }
            ListOfChecked.Sort();
            ListOfUnChecked.Sort();
            if (Type_Of_Command == "sb_UnapprovedHSVoiceEnabled")
                ProcessUnapproved();
            else if (Type_Of_Command == "sb_NotVoiceEnabledInHSButAreInINI")
                ProcessNotVoiceEnabledInHS();
            else if (Type_Of_Command == "sb_BothApprovedAndInHS")
                ProcessBothApprovedAndInHS();
            this.pageCommands.Add("refresh", "true");
            return base.postBackProc(page, data, user, userRights);
        }
        private string Generate_Table_Row(int Reference)
        {
            StringBuilder stb = new StringBuilder();
            string DeviceFullName = DictionaryOfRefToFullDeviceName[Reference];
            string DeviceVoiceCommand = DictionaryOfRefToVoiceCommand[Reference];
            string DeviceNameHyperLinkText = "<a href='http://" + HomeSeerIPAddress + ":" + HomeSeerWebServerPort + "/deviceutility?ref=" + Reference.ToString() + "&edit=1' target='_blank'>" + DeviceFullName + "</a>";
            string Checkbox_HTML = "<input id='scb_" + Reference.ToString() + "' type='checkbox' value='checked' name='vrr_" + Reference.ToString() + "' onchange='update_hidden_field(this);' checked><label for='" + Reference.ToString() + "'>" + DeviceNameHyperLinkText + "</label>";
            string Checkbox_Hidden = "<input id='hcb_" + Reference.ToString() + "' type='hidden' value='unchecked' name='vrr_" + Reference.ToString() + "' disabled>";
            stb.Append("<tr><td>");
            stb.Append("<input type='text' id='vct_" + Reference.ToString() + "' name='vct_" + Reference.ToString() + "' value='" + DeviceVoiceCommand + "'>");
            stb.Append("</td><td>");
            stb.Append(Checkbox_HTML);
            stb.Append(Checkbox_Hidden);
            stb.Append("</td><tr>");
            return stb.ToString();
        }
        private string Populate_Voice_Entries_In_Page_Section(string PageShortURL, List<int> List_To_Process, string Section, string Title, string Checked_Behavior, string UnChecked_Behavior)
        {
            StringBuilder stb = new StringBuilder();
            if (List_To_Process.Count > 0)
            {
                stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("form_" + Section, PageName, "post"));
                stb.Append("<div id='div_" + Section + "' class='" + Section + "'><tr><td align='center'><h3>" + Title + "</h3><table id='table_" + Section + "' class='" + Section + "'>");
                stb.Append("<input type='button' id='Toggle_Checkboxes_" + Section + "' onclick='Toggle_Button_Value(this);' name='" + Section + "' value='UnCheck All'/>");
                List<string> List_Of_Voice_Devices = new List<string>();
                foreach (int Reference in List_To_Process)
                {
                    string DeviceFullName = DictionaryOfRefToFullDeviceName[Reference];
                    List_Of_Voice_Devices.Add(DeviceFullName + "~" + Generate_Table_Row(Reference));
                }
                List_Of_Voice_Devices.Sort();
                foreach (string Voice_Device_Raw in List_Of_Voice_Devices)
                {
                    string[] Voice_Device_Parts = Voice_Device_Raw.Split('~');
                    string Voice_Device_HTML = Voice_Device_Parts[1];
                    stb.Append(Voice_Device_HTML);
                }
                stb.Append("</table>");
                stb.Append("<br />Total Count: " + List_To_Process.Count + "<br /><br />");
                stb.Append("<p><span style='border-style:solid;border-color:#287EC7;background-color: #E3E3E3;'><span style='color: red;'>Voice Command Text Box:</span> Any Changes Will Be Saved To The Device</span></p>");
                stb.Append("<p><span style='border-style:solid;border-color:#287EC7;background-color: #E3E3E3;'><span style='color: red;'>Checked Checkbox:</span> " + Checked_Behavior + "</span></p>");
                stb.Append("<p><span style='border-style:solid;border-color:#287EC7;background-color: #E3E3E3;'><span style='color: red;'>UnChecked Checkbox:</span> " + UnChecked_Behavior + "</span></p>");
                clsJQuery.jqButton submit_button = new clsJQuery.jqButton("sb_" + Section, "Process " + Title, PageShortURL, true);
                stb.Append(submit_button.Build());
                stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());
                stb.Append("<hr /></td><tr></div>");
            }
            return stb.ToString();
        }
        private void PopulateAllLists()
        {
            PopulateListOfAllCurrentHSVoiceEnabledRefs();
            PopulateListOfINIApprovedRefs();
            PopulateListOfUnapprovedVoiceRefs();
            PopulateListOfValidDevicesInINIButNotVoiceInHSRefs();
            PopulateListOfApproveVoiceInINIandAlsoVoiceInHSRefs();
            PopulateDictionaryOfRefToFullDeviceNameAndVoiceCommand();
        }
        private void PopulateListOfAllCurrentHSVoiceEnabledRefs()
        {
            ListOfAllCurrentHSVoiceEnabledRefs.Clear();
            Scheduler.Classes.DeviceClass DeviceObject;
            Scheduler.Classes.clsDeviceEnumeration DeviceEnumeration;
            DeviceEnumeration = (Scheduler.Classes.clsDeviceEnumeration)hs.GetDeviceEnumerator();
            do
            {
                DeviceObject = DeviceEnumeration.GetNext();
                if (DeviceObject == null)
                    continue;
                if (DeviceObject.MISC_Check(hs, HomeSeerAPI.Enums.dvMISC.AUTO_VOICE_COMMAND))
                    ListOfAllCurrentHSVoiceEnabledRefs.Add(DeviceObject.get_Ref(hs));
            }
            while (!DeviceEnumeration.Finished);
            ListOfAllCurrentHSVoiceEnabledRefs.Sort();
        }
        private void PopulateListOfINIApprovedRefs()
        {
            ListOfINIApprovedRefs.Clear();
            string[] Items;
            Items = hs.GetINISectionEx("Valid Voice Enabled Devices", INIFileName);
            if (Items != null && Items.Length > 0)
            {
                foreach (string Entry in Items)
                {
                    if (string.IsNullOrEmpty(Entry))
                        continue;
                    string DeviceReferenceText;
                    string DeviceEnabledText;
                    string[] EntryElements = Entry.Split(new char[] { '=' });
                    DeviceReferenceText = EntryElements[0].Trim();
                    DeviceEnabledText = EntryElements[1].Trim();
                    int Reference = System.Convert.ToInt32(DeviceReferenceText);
                    if (DeviceEnabledText != "0")
                    {
                        if (hs.DeviceExistsRef(Reference))
                            ListOfINIApprovedRefs.Add(Reference);
                        else
                            RemoveDeviceFromINIFIle(Reference);
                    }
                }
            }
            ListOfINIApprovedRefs.Sort();
        }
        private void RemoveDeviceFromINIFIle(int Reference)
        {
            hs.SaveINISetting("Valid Voice Enabled Devices", Reference.ToString(), "0", INIFileName);
        }
        private void AddDeviceToINIFile(int Reference)
        {
            hs.SaveINISetting("Valid Voice Enabled Devices", Reference.ToString(), "1", INIFileName);
        }
        private void PopulateListOfUnapprovedVoiceRefs()
        {
            ListOfUnapprovedVoiceRefs.Clear();
            foreach (int Reference in ListOfAllCurrentHSVoiceEnabledRefs)
            {
                if (!ListOfINIApprovedRefs.Contains(Reference))
                    ListOfUnapprovedVoiceRefs.Add(Reference);
            }
            ListOfUnapprovedVoiceRefs.Sort();
        }
        private void PopulateListOfValidDevicesInINIButNotVoiceInHSRefs()
        {
            ListOfValidDevicesInINIButNotVoiceInHSRefs.Clear();
            foreach (int Reference in ListOfINIApprovedRefs)
            {
                if (!ListOfAllCurrentHSVoiceEnabledRefs.Contains(Reference))
                    ListOfValidDevicesInINIButNotVoiceInHSRefs.Add(Reference);
            }
            ListOfValidDevicesInINIButNotVoiceInHSRefs.Sort();
        }
        private void PopulateListOfApproveVoiceInINIandAlsoVoiceInHSRefs()
        {
            ListOfApproveVoiceInINIandAlsoVoiceInHSRefs.Clear();
            foreach (int Reference in ListOfINIApprovedRefs)
            {
                if (ListOfAllCurrentHSVoiceEnabledRefs.Contains(Reference))
                    ListOfApproveVoiceInINIandAlsoVoiceInHSRefs.Add(Reference);
            }
            ListOfApproveVoiceInINIandAlsoVoiceInHSRefs.Sort();
        }
        private void PopulateDictionaryOfRefToFullDeviceNameAndVoiceCommand()
        {
            DictionaryOfRefToFullDeviceName.Clear();
            DictionaryOfRefToVoiceCommand.Clear();
            List<int> MasterListOfRefsWithDupes = new List<int>();
            MasterListOfRefsWithDupes.AddRange(ListOfAllCurrentHSVoiceEnabledRefs);
            MasterListOfRefsWithDupes.AddRange(ListOfINIApprovedRefs);
            MasterListOfRefsWithDupes.Sort();
            List<int> MasterListOfRefs = MasterListOfRefsWithDupes.Distinct().ToList();
            MasterListOfRefs.Sort();
            foreach (int Reference in MasterListOfRefs)
            {
                if (hs.DeviceExistsRef(Reference))
                {
                    DictionaryOfRefToFullDeviceName[Reference] = hs.DeviceName(Reference).Trim();
                    DictionaryOfRefToVoiceCommand[Reference] = ((Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(Reference)).get_VoiceCommand(hs);
                }
                else
                {
                    DictionaryOfRefToFullDeviceName[Reference] = "Device Does Not Exist In HomeSeer";
                    DictionaryOfRefToVoiceCommand[Reference] = "Device Does Not Exist In HomeSeer";
                }
            }
        }
        private void ProcessUnapproved()
        {
            foreach (int Reference in ListOfChecked)
                AddDeviceToINIFile(Reference);
            foreach (int Reference in ListOfUnChecked)
                ((Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(Reference)).MISC_Clear(hs, HomeSeerAPI.Enums.dvMISC.AUTO_VOICE_COMMAND);
        }
        private void ProcessNotVoiceEnabledInHS()
        {
            foreach (int Reference in ListOfChecked)
                ((Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(Reference)).MISC_Set(hs, HomeSeerAPI.Enums.dvMISC.AUTO_VOICE_COMMAND);
            foreach (int Reference in ListOfUnChecked)
                RemoveDeviceFromINIFIle(Reference);
        }
        private void ProcessBothApprovedAndInHS()
        {
            foreach (int Reference in ListOfUnChecked)
            {
                RemoveDeviceFromINIFIle(Reference);
                ((Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(Reference)).MISC_Clear(hs, HomeSeerAPI.Enums.dvMISC.AUTO_VOICE_COMMAND);
            }
        }
        private void Check_And_Modify_Voice_Command_As_Needed(string Field_Name, string Input_Voice_Command_Text)
        {
            int Reference = System.Convert.ToInt32(Field_Name.Replace("vct_", ""));
            string Previous_Voice_Command_Text = DictionaryOfRefToVoiceCommand[Reference].Trim();
            if (Input_Voice_Command_Text.Trim() != Previous_Voice_Command_Text)
                ((Scheduler.Classes.DeviceClass)hs.GetDeviceByRef(Reference)).set_VoiceCommand(hs, Input_Voice_Command_Text.Trim());
        }
    }
}