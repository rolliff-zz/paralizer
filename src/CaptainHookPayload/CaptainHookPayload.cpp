// CaptainHookPayload.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "..\CaptainHook\Hooker.h"

extern "C" void _cdecl Payload(void* data, size_t len)
{
  hooker::DefaultPayload(data,len);
}


