////////////////////////////////////////////////////////////////////////////////
/// Winamp C# Plugin Wrapper - Part of Sharpamp C# library
////////////////////////////////////////////////////////////////////////////////
#include "StdAfx.h"
#include "PluginWrapper.h"

using namespace System::Runtime::InteropServices;

char * PluginWrapper::Name()
{
	// Convert .NET String to C char array
	return static_cast<char *>((Marshal::StringToHGlobalAnsi(PluginWrapper::plugin->Name).ToPointer()));
}