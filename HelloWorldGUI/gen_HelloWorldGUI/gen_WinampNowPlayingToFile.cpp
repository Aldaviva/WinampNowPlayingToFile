////////////////////////////////////////////////////////////////////////////////
/// Wrapper for HelloWorldGUI
////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "gen_WinampNowPlayingToFile.h"
#include "PluginWrapper.h"

using namespace System;

// this structure contains plugin information, version, name...
winampGeneralPurposePlugin plugin =
{
	GPPHDR_VER,  // version of the plugin, defined in "gen_HelloWorldPlugin.h"
	PluginWrapper::Name(),
	init,        // function name which will be executed on init event
	config,      // function name which will be executed on config event
	quit,        // function name which will be executed on quit event
	0,           // handle to Winamp main window, loaded by winamp when this dll is loaded
	0            // hinstance to this dll, loaded by winamp when this dll is loaded
};
 
// Called when Winamp initializes the plugin
int init()
{
	try
	{
		PluginWrapper::plugin->Init(safe_cast<IntPtr>(plugin.hwndParent));
	}
	catch (Exception^ ex)
	{
		System::Windows::Forms::MessageBox::Show(L"An error occured while initialising! \r\n" + ex->Message);
		return 1;
	}

	return 0;
}
 
// Called when Configure button in Winamp is clicked
void config()
{
	PluginWrapper::plugin->Config();
}
 
// Called when Winamp is quitting
void quit()
{
	PluginWrapper::plugin->Quit();
	delete PluginWrapper::plugin;
}

// Return data about the plugin to Winamp
extern "C" __declspec(dllexport) winampGeneralPurposePlugin * winampGetGeneralPurposePlugin()
{
	return &plugin;
}