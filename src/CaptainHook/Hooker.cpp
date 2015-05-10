#include "stdafx.h"
#include "Hooker.h"


namespace {
  struct Input
  {
    char module_name[100];  
    char function_name[100];
    size_t data_len;
    char data;
  };
}


DWORD ThreadStart( PVOID Data )
{
  __debugbreak();
//  ::Load
  return 0;
}

hooker::WINERR DamnedError()
{
  auto x = ::GetLastError();
  printf("Crap %d\n", x);
  return x;
}
hooker::WINERR hooker::Hook(
      // Attach to this process id
      size_t target_process_id,

      // Load this DLL
      const std::string& module_name,
    
      // Execute this exported function
      // Must have signature of target_function above.
      const std::string& function_name,

      // Pass this data as param
      void* the_data,

      size_t data_length
    )
{

  size_t input_length = sizeof(Input) + data_length - 1;
  Input* n = (Input*)malloc(input_length);

  auto access 
    = PROCESS_CREATE_THREAD 
    | PROCESS_VM_OPERATION 
    | PROCESS_VM_WRITE 
    | PROCESS_VM_READ 
    | PROCESS_QUERY_INFORMATION
    ;

  memcpy(n->module_name, module_name.c_str(), module_name.length()+1);
  memcpy(n->function_name, function_name.c_str(), function_name.length()+1);
  n->data_len = data_length;
  memcpy( ((void*)&n->data), the_data, data_length);

  auto process_handle = ::OpenProcess(access, FALSE, target_process_id);
  if(process_handle == NULL || process_handle == INVALID_HANDLE_VALUE)
    return DamnedError();
  

  auto playground = ::VirtualAllocEx(process_handle,NULL, input_length,MEM_COMMIT, PAGE_EXECUTE_READWRITE);
  if(nullptr == playground)
    return DamnedError();

  SIZE_T written(0);
  if(FALSE == ::WriteProcessMemory(process_handle,playground,(void*)n, input_length, &written))
    return DamnedError();

  //::CreateRemoteThread(process_handle, NULL, NULL, 

  return NOERROR;
}


void hooker::DefaultPayload(void* data, size_t data_length)
{ 
  std::string s((char*)data);
  printf("the string is %s\n", s.c_str());
}