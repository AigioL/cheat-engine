unit Filehandler;

{$MODE Delphi}

{
implement replaced handlers for ReadProcssMemory and WriteProcessMemory so it
reads/writes to the file instead
}

interface

uses windows, LCLIntf, syncobjs;

function ReadProcessMemoryFile(hProcess: THandle; const lpBaseAddress: Pointer; lpBuffer: Pointer;  nSize: DWORD; var lpNumberOfBytesRead: PTRUINT): BOOL; stdcall;
function WriteProcessMemoryFile(hProcess: THandle; const lpBaseAddress: Pointer; lpBuffer: Pointer; nSize: DWORD; var lpNumberOfBytesWritten: PTRUINT): BOOL; stdcall;
function VirtualQueryExFile(hProcess: THandle; lpAddress: Pointer; var lpBuffer: TMemoryBasicInformation; dwLength: DWORD): DWORD; stdcall;

var filehandle: thandle;

implementation

var filecs: tcriticalsection; //only 1 filehandle, so make sure rpm does not change the filepointer while another is still reading it

function GetCurrentFileSize: qword;
begin
  result:=0;
  if (filehandle=0) or (filehandle=INVALID_HANDLE_VALUE) then exit;

  result:=GetFileSize(filehandle, pointer(ptruint(@result)+4));
end;

function SetCurrentFilePointer(offset: qword): boolean;
var
  highoffset: dword;
  lowoffset: dword;
begin
  highoffset:=offset shr 32;
  lowoffset:=offset and $ffffffff;
  lowoffset:=SetFilePointer(filehandle, lowoffset, @highoffset, FILE_BEGIN);
  result:=(lowoffset<>$ffffffff) or (GetLastError=NO_ERROR);
end;

function ReadProcessMemoryFile(hProcess: THandle; const lpBaseAddress: Pointer; lpBuffer: Pointer;  nSize: DWORD; var lpNumberOfBytesRead: PTRUINT): BOOL; stdcall;
var
  filesize: qword;
  sizetoread: dword;
  bytesread: dword;
begin
//ignore hprocess
  lpNumberOfBytesRead:=0;
  result:=false;
  if (filehandle=0) or (filehandle=INVALID_HANDLE_VALUE) then exit;

  filesize:=GetCurrentFileSize;
  if ptrUint(lpbaseaddress)>=filesize then exit;

  sizetoread:=nsize;

  if qword(ptrUint(lpbaseaddress))+sizetoread>filesize then
  begin
    ZeroMemory(lpBuffer, nsize);
    sizetoread:=filesize-ptrUint(lpbaseaddress);
  end;

  if sizetoread=0 then exit;

  filecs.enter;
  try
    if not SetCurrentFilePointer(ptrUint(lpBaseAddress)) then exit;

    bytesread:=0;
    result:=Readfile(filehandle,lpbuffer^,sizetoread,bytesread,nil);
    lpNumberOfBytesRead:=bytesread;
  finally
    filecs.leave;
  end;
end;

function WriteProcessMemoryFile(hProcess: THandle; const lpBaseAddress: Pointer; lpBuffer: Pointer; nSize: DWORD; var lpNumberOfBytesWritten: PTRUINT): BOOL; stdcall;
var
  filesize: qword;
  sizetowrite: dword;
  byteswritten: dword;
begin
  lpNumberOfBytesWritten:=0;
  result:=false;
  if (filehandle=0) or (filehandle=INVALID_HANDLE_VALUE) then exit;

  filesize:=GetCurrentFileSize;
  if ptrUint(lpbaseaddress)>=filesize then exit;

  sizetowrite:=nsize;

  if qword(ptrUint(lpbaseaddress))+sizetowrite>filesize then
    sizetowrite:=filesize-ptrUint(lpbaseaddress);

  if sizetowrite=0 then exit;

  filecs.enter;
  try
    if not SetCurrentFilePointer(ptrUint(lpBaseAddress)) then exit;

    byteswritten:=0;
    result:=Writefile(filehandle,lpbuffer^,sizetowrite,byteswritten,nil);
    lpNumberOfBytesWritten:=byteswritten;
  finally
    filecs.leave;
  end;
end;

function VirtualQueryExFile(hProcess: THandle; lpAddress: Pointer; var lpBuffer: TMemoryBasicInformation; dwLength: DWORD): DWORD; stdcall;
var
  filesize: qword;
begin
  if (filehandle=0) or (filehandle=INVALID_HANDLE_VALUE) then
  begin
    zeromemory(@lpbuffer,dwlength);
    exit(0);
  end;

  filesize:=GetCurrentFileSize;
  if ptrUint(lpAddress)>=filesize then
  begin
    zeromemory(@lpbuffer,dwlength);
    exit(0);
  end;

  lpBuffer.BaseAddress:=pointer((ptrUint(lpAddress) div $1000)*$1000);
  lpbuffer.AllocationBase:=pointer(0);
  lpbuffer.AllocationProtect:=PAGE_EXECUTE_READWRITE;
  lpbuffer.RegionSize:=ptruint(filesize-ptrUint(lpBuffer.BaseAddress));
  if (lpbuffer.RegionSize mod $1000)>0 then
    lpbuffer.RegionSize:=lpbuffer.RegionSize+($1000-lpbuffer.RegionSize mod $1000);


  lpbuffer.State:=mem_commit;
  lpbuffer.Protect:=PAGE_EXECUTE_READWRITE;
  lpbuffer._Type:=MEM_PRIVATE;

  result:=dwlength;

end;

initialization
  filecs:=tcriticalsection.create;

finalization
  filecs.free;


end.






