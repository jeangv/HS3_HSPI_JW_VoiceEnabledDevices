# HS3_HSPI_JW_VoiceEnabledDevices
HomeSeer 3 - Plugin For Managing Voice Enabled Devices

I had originally written this code as an ASPX page here but HS3's support for ASPX was not very good and it would hang from time to time. I have completely rewritten the code as a native HS3 Plugin in C#.

When controlling HomeSeer via Voice Commands (through the Amazon Echo Alexa or Google Home) each device has a setting called "Voice Command" that is for some crazy reason enabled by default for every single device that is created. So any device created by you or created by a plug-in, will have this "Voice Command" enabled by default.

This Plugin allows you to view only the HomeSeer Devices that are Voice Enabled (via the "Voice Command" setting) and create a list of "approved" voice enabled devices and then remove the Voice Command setting from all other devices.

One of the major pain points with HomeSeer 3 (HS3) is the lack of a simple way to view and modify all of your voice enabled devices. This problem is compounded buy the fact that HS3 automatically by default voice enables every device created. Whether that device is created manually or by a plugin makes no difference, it will be automatically voice enabled whether you want it to be or not.

# How To Install The Plugin on HS3:
Unfortunately, HomeSeer no longer allows for publishing new HS3 Plugins to the updater any longer as all plugins move to HS4 only. You will need to install this plugin manually, but it is a simple process. Just download the Plugin file from the GitHib Releases. Once downloaded, UnZip the file and copy the two files (HSPI_JW_VoiceEnabledDevices.exe and HSPI_JW_VoiceEnabledDevices.exe.config) into the root of your HS3 directory. Once done you can enable the Plugin.

# How To Use The Plugin:
Once the plugin is enabled, you will see a new menu item available from your "PLUG-INS" menu. Navigate there and click on it. After a few seconds the JeanWare HS3 Voice Enabled Device Management will display all your voice enabled devices and you can choose to add them to the "approved" list or remove them from being voice enabled. Each section explains the various options, and this is saved.

This should work on HS3 and HS4 as well as Windows and Linux

Please let me know if you have any problems or questions as well as any feature requests here on GitHub.

There is also a HomeSeer forum post about this plugin here:
https://forums.homeseer.com/forum/developer-support/scripts-plug-ins-development-and-libraries/script-plug-in-library/1457331-free-plugin-hs3-hs4-view-manage-homeseer-devices-voice-command-enabled-setting
