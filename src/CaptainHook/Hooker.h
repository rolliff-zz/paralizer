#ifndef _HOOKER_H_
#define _HOOKER_H_

#include <Windows.h>
#include <string>
namespace hooker
{
  typedef DWORD WINERR;
  typedef void (_cdecl* target_function)(void* data, size_t data_length);

  void DefaultPayload(void* data, size_t data_length);

  WINERR Hook(
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
    );

}

#endif // !_HOOKER_H_
