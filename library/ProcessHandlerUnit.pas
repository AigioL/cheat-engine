unit ProcessHandlerUnit;

{$MODE Delphi}

{
Will handle all process specific stuff like openening and closing a process
The ProcessHandler variable will be in cefuncproc, but a tabswitch to another
process will set it to the different tab's process
}

interface

uses LCLIntf, newkernelhandler, classes;

type
  TSystemArchitecture=(archX86=0, archArm=1);
  TOperatingsystemABI=(abiWindows=0, abiSystemV=1);

type TProcessHandler=class
  private
    fis64bit: boolean;
    fprocesshandle: THandle;
    fpointersize: integer;
    fSystemArchitecture: TSystemArchitecture;
    fOSABI: TOperatingsystemABI;
    fHexDigitPreference: integer;
    procedure setIs64bit(state: boolean);
    procedure setProcessHandle(processhandle: THandle);
  public
    processid: dword;


    procedure Open;
    procedure overridePointerSize(newsize: integer);

    property is64Bit: boolean read fIs64Bit write setIs64bit;
    property pointersize: integer read fPointersize;
    property processhandle: THandle read fProcessHandle write setProcessHandle;
    property SystemArchitecture: TSystemArchitecture read fSystemArchitecture write fSystemArchitecture;
    property OSABI: TOperatingsystemABI read fOSABI;
    property hexdigitpreference: integer read fHexDigitPreference;
end;

implementation

procedure TProcessHandler.overridePointerSize(newsize: integer);
begin
  fpointersize:=newsize;
end;

procedure TProcessHandler.setIs64bit(state: boolean);
begin
  fis64bit:=state;
  if state then
  begin
    fpointersize:=8;
  end
  else
  begin
    fpointersize:=4;
  end;

  fHexDigitPreference:=fPointersize*2;
end;

procedure TProcessHandler.setProcessHandle(processhandle: THandle);
begin
  fprocesshandle:=processhandle;
  fSystemArchitecture:=archX86;
  fOSABI:=abiWindows;
  setIs64Bit(newkernelhandler.Is64BitProcess(fProcessHandle));

//  if (mainform<>nil) and (mainform.addresslist<>nil) then
//    mainform.addresslist.needsToReinterpret:=true;
end;

procedure TProcessHandler.Open;
begin

end;

end.

