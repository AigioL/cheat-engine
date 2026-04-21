unit connector;

{$MODE Delphi}

interface

uses
  Classes, SysUtils, symbolhandler, CEFuncProc, windows, PEInfounit,
  MemoryRecordDatabase, MemoryRecordUnit, Dialogs,
  memscan,scanner, CustomTypeHandler, LazUTF8;

//procedure ILoadCommonModuleList; stdcall;
procedure IGetProcessList(out processes : WideString);stdcall;
procedure IGetModuleList(withSystemModules: boolean ; out modules : WideString);stdcall;
procedure IOpenProcess(pid : WideString);stdcall;

//control the virtual cheat table
procedure IResetTable();stdcall;
procedure IAddScript(name : WideString; script : WideString);stdcall;
procedure IRemoveRecord(id : integer); stdcall;
procedure IActivateRecord(id : integer; activate : boolean);stdcall;
procedure IApplyFreeze();stdcall;

//control an address from the virtual cheat table
procedure IAddAddressManually(initialaddress: WideString=''; vartype: TVariableType=vtDword);stdcall;
procedure IGetValue(id : integer ; out value : WideString);stdcall;
procedure ISetValue(id : integer ; value : WideString ; freezer : boolean);stdcall;

//control the memory scanner
procedure IInitMemoryScanner(hwnd : THandle); stdcall;
procedure INewScan();stdcall;
procedure IConfigScanner(scanWritable: Tscanregionpreference;scanExecutable: Tscanregionpreference;scanCopyOnWrite: Tscanregionpreference);stdcall;
procedure IFirstScan(scanOption: TScanOption; VariableType: TVariableType;
  roundingtype: TRoundingType; scanvalue1, scanvalue2: WideString; startaddress,stopaddress: WideString;
  hexadecimal,binaryStringAsDecimal,unicode,casesensitive: boolean; fastscanmethod: TFastScanMethod=fsmNotAligned;
  fastscanparameter: WideString=''); stdcall;

procedure INextScan(scanOption: TScanOption; roundingtype: TRoundingType;
  scanvalue1, scanvalue2: WideString;
  hexadecimal,binaryStringAsDecimal, unicode, casesensitive,percentage,compareToSavedScan: boolean;
  savedscanname: WideString); stdcall; //next scan, determine what kind of scan and give to firstnextscan/nextnextscan

function ICountAddressesFound():uint64; stdcall;
procedure IGetAddress(i: qword;out address: WideString; out value: WideString)stdcall;
procedure IInitFoundList(vartype: TVariableType; varlength: integer; hexadecimal,
  signed,binaryasdecimal,unicode: boolean);stdcall;
procedure IResetValues;stdcall;
procedure IRebaseAddressList(index : integer);stdcall;
function IGetBinarySize():integer;stdcall;

var
  recordTable : TMemoryRecordTable;
  scanner_ : TIScanner;

resourcestring
  rsAutoAssembleScript = 'Auto Assemble script';
  rsIsnTAValidProcessID = '%s isn''t a valid processID';

implementation

function BStrToCEAnsiString(const value: WideString): string;
begin
  Result:=UTF8ToAnsi(UTF16ToUTF8(value));
end;

function TryBStrToAsciiString(const value: WideString; out asciiValue: string): boolean;
var
  i: Integer;
begin
  SetLength(asciiValue, Length(value));

  for i:=1 to Length(value) do
  begin
    if Ord(value[i])>$7f then
    begin
      asciiValue:='';
      Result:=false;
      exit;
    end;

    asciiValue[i]:=AnsiChar(Ord(value[i]));
  end;

  Result:=true;
end;

function BStrToCEAsciiOrAnsiString(const value: WideString): string;
begin
  if not TryBStrToAsciiString(value, Result) then
    Result:=BStrToCEAnsiString(value);
end;

function BStrToCEUTF8String(const value: WideString): string;
begin
  Result:=UTF16ToUTF8(value);
end;

function CEUTF8StringToBStr(const value: string): WideString;
begin
  Result:=UTF8ToUTF16(value);
end;

function CEAnsiStringToBStr(const value: string): WideString;
begin
  Result:=UTF8ToUTF16(AnsiToUTF8(value));
end;

procedure GetEntryPointAndDataBase(var code: ptrUint; var data: ptrUint);
var modulelist: tstringlist=nil;
    base: ptrUint;
  baseaddress: pointer;
    header: pointer=nil;
    headersize: dword;
  br: ptrUint;
begin
  code:=$00400000;
  data:=$00400000; //on failure
  br:=0;

  modulelist:=tstringlist.Create;
  try
    symhandler.getModuleList(modulelist);
    outputdebugstring('Retrieved the module list');

    if modulelist.Count>0 then
    begin
      baseaddress:=modulelist.Objects[0];
      base:=ptrUint(baseaddress);


      getmem(header,4096);
      try
        if readprocessmemory(processhandle,baseaddress,header,4096,br) then
        begin
          headersize:=peinfo_getheadersize(header);

          if headersize>0 then
          begin
            if headersize>1024*512 then exit;

            freememandnil(header);
            getmem(header,headersize);
            if not readprocessmemory(processhandle,baseaddress,header,headersize,br) then exit;

            Outputdebugstring('calling peinfo_getEntryPoint');
            code:=base+peinfo_getEntryPoint(header, headersize);

            OutputDebugString('calling peinfo_getdatabase');
            data:=base+peinfo_getdatabase(header, headersize);
          end;


        end;
      finally
        if header<>nil then
          freememandnil(header);
      end;
    end;
  finally
    modulelist.free;
  end;
end;

procedure setcodeanddatabase;
var code,data: ptrUint;
begin
  code:=0;
  data:=0;

  if processid=$ffffffff then  //file instead of process
  begin
    code:=0;
    data:=0;
  end
  else
    GetEntryPointAndDataBase(code,data);
end;

procedure openProcessEpilogue();
begin
     symhandler.reinitialize;
    setcodeanddatabase;
end;

procedure PWOP(ProcessIDString:string);
var i:integer;
begin
  val('$'+ProcessIDString,ProcessHandler.processid,i);
  if i<>0 then
    raise exception.Create(Format(rsIsnTAValidProcessID, [ProcessIDString]));
  if Processhandle<>0 then
  begin
    CloseHandle(ProcessHandle);
    ProcessHandler.ProcessHandle:=0;
  end;
  Open_Process;
  ProcessSelected:=true;
end;

procedure ILoadCommonModuleList; stdcall;
begin
     symhandler.loadCommonModuleList;
end;

procedure IGetModuleList(withSystemModules: boolean ; out modules : WideString);stdcall;
var
  _modules : TStringList;
begin
  _modules:=nil;
  try
    _modules := TStringList.Create;
    GetModuleList(_modules,withSystemModules);
    modules := CEAnsiStringToBStr(_modules.Text);
  finally
    if _modules<>nil then
      _modules.Free;
  end;
end;

procedure IGetProcessList(out processes : WideString);stdcall;
var
  process : TStringList;
begin
  process:=nil;
  try
     process := TStringList.Create();
     GetProcessList(process);
      processes := CEUTF8StringToBStr(process.Text);
  finally
     if process<>nil then
       process.Free;
  end;
end;

procedure IOpenProcess(pid : WideString);stdcall;
begin
     IResetTable();
      PWOP(BStrToCEAsciiOrAnsiString(pid));
     openProcessEpilogue();
     symhandler.reinitialize();
     symhandler.waitforsymbolsloaded;
     symhandler.loadmodulelist;
end;

procedure IResetTable();stdcall;
begin
  if recordTable <> nil then
  begin
    recordTable.Free;
    recordTable := nil;
  end;
  recordTable := TMemoryRecordTable.Create();
end;

procedure IAddScript(name : WideString; script : WideString); stdcall;
var
  scriptDescription: string;
begin
  if recordTable <> nil then
  begin
    scriptDescription:=BStrToCEUTF8String(name);
    if scriptDescription='' then
      scriptDescription:=rsAutoAssembleScript;

    recordTable.addAutoAssembleScript(scriptDescription,BStrToCEUTF8String(script));
  end;
end;

procedure IRemoveRecord(id : integer); stdcall;
begin
  if recordTable <> nil then
  begin
    recordTable.RemoveRecord(id);
  end;
end;

procedure IActivateRecord(id : integer; activate : boolean);stdcall;
var
  memrec : TMemoryRecord;
begin
  if recordTable=nil then exit;

  memrec := recordTable.getRecordWithID(id);
  if memrec=nil then exit;

  memrec.isSelected:=true;
  if activate then
     recordTable.ActivateSelected(ftFrozen)
  else
     recordTable.DeactivateSelected;
  memrec.isSelected:=false;
end;

procedure IApplyFreeze();stdcall;
begin
  if recordTable<>nil then
    recordTable.ApplyFreeze();
end;

procedure IAddAddressManually(initialaddress: WideString=''; vartype: TVariableType=vtDword);stdcall;
begin
  if recordTable <> nil then
    begin
      recordTable.addAddressManually(BStrToCEAsciiOrAnsiString(initialaddress),vartype);
    end;
end;

procedure IGetValue(id : integer ; out value : WideString);stdcall;
var
  memrec : TMemoryRecord;
begin
    value := '';

    if recordTable=nil then exit;

    memrec := recordTable.getRecordWithID(id);
    if memrec=nil then exit;

    if (memrec.VarType=vtString) and (not memrec.Extra.stringData.unicode) then
      value := CEAnsiStringToBStr(memrec.GetValue)
    else
      value := CEUTF8StringToBStr(memrec.GetValue);
end;

procedure ISetValue(id : integer ; value : WideString ; freezer : boolean);stdcall;
var
  memrec : TMemoryRecord;
  value_: string;
begin
    if recordTable=nil then exit;

    memrec := recordTable.getRecordWithID(id);
    if memrec=nil then exit;

    if (memrec.VarType=vtString) and memrec.Extra.stringData.unicode then
      value_ := BStrToCEUTF8String(value)
    else
      value_ := BStrToCEAnsiString(value);

    memrec.SetValue(value_,freezer);
end;

procedure IInitMemoryScanner(hwnd : THandle); stdcall;
begin
  if (assigned(scanner_)) then
  begin
    scanner_.Free;
  end;
  scanner_ := TIScanner.Create(hwnd);
end;

procedure IInitFoundList(vartype: TVariableType; varlength: integer; hexadecimal,
  signed,binaryasdecimal,unicode: boolean); stdcall;
begin
  if vartype=vtUnicodeString then
  begin
    vartype:=vtString;
    unicode:=true;
  end;

  if assigned(scanner_) then
  begin
    if vartype=vtCustom then
      scanner_.foundlist.Initialize(vartype,varlength,hexadecimal,signed,binaryasdecimal,unicode,scanner_.memscan.CustomType)
    else
      scanner_.foundlist.Initialize(vartype,varlength,hexadecimal,signed,binaryasdecimal,unicode,nil);
  end;
end;

procedure INewScan();stdcall;
begin
  if assigned(scanner_) then
    scanner_.memscan.newscan;
end;

procedure IFirstScan(scanOption: TScanOption; variableType: TVariableType;
  roundingtype: TRoundingType; scanvalue1, scanvalue2: WideString; startaddress,stopaddress: WideString;
  hexadecimal,binaryStringAsDecimal,unicode,casesensitive: boolean; fastscanmethod: TFastScanMethod=fsmNotAligned;
  fastscanparameter: WideString=''); stdcall;
var
  scanstart,scanend : PtrUint;
  v1,v2:string;
begin
  if not assigned(scanner_) then exit;

  if variableType=vtUnicodeString then
  begin
    variableType:=vtString;
    unicode:=true;
  end;

  scanstart := StrToQWordEx(BStrToCEAsciiOrAnsiString(startaddress));
  scanend := StrToQWordEx(BStrToCEAsciiOrAnsiString(stopaddress));

  if (variableType=vtString) and unicode then
  begin
    v1 := BStrToCEUTF8String(scanvalue1);
    v2 := BStrToCEUTF8String(scanvalue2);
  end
  else
  begin
    v1 := BStrToCEAnsiString(scanvalue1);
    v2 := BStrToCEAnsiString(scanvalue2);
  end;

  scanner_.memscan.ScanOption:=scanOption;
  scanner_.memscan.VarType:=variableType;
  scanner_.memscan.Roundingtype:=roundingtype;
  scanner_.memscan.Scanvalue1:=v1;
  scanner_.memscan.Scanvalue2:=v2;
  scanner_.memscan.Startaddress:=scanstart;
  scanner_.memscan.Stopaddress:=scanend;
  scanner_.memscan.Hexadecimal:=hexadecimal;
  scanner_.memscan.BinaryStringAsDecimal:=binaryStringAsDecimal;
  scanner_.memscan.Unicode:=unicode;
  scanner_.memscan.Casesensitive:=casesensitive;
  scanner_.memscan.Fastscanmethod:=fastscanmethod;
  scanner_.memscan.Fastscanparameter:=BStrToCEAsciiOrAnsiString(fastscanparameter);
  scanner_.memscan.CustomType:=nil;
  scanner_.memscan.FirstScan;
end;

procedure INextScan(scanOption: TScanOption; roundingtype: TRoundingType;
  scanvalue1, scanvalue2: WideString;
  hexadecimal,binaryStringAsDecimal, unicode, casesensitive,percentage,compareToSavedScan: boolean;
  savedscanname: WideString); stdcall;
var
  v1,v2:string;
begin
  if assigned(scanner_) then
  begin
    if unicode and (scanner_.memscan.VarType=vtString) then
    begin
      v1 := BStrToCEUTF8String(scanvalue1);
      v2 := BStrToCEUTF8String(scanvalue2);
    end
    else
    begin
      v1 := BStrToCEAnsiString(scanvalue1);
      v2 := BStrToCEAnsiString(scanvalue2);
    end;

    scanner_.memscan.ScanOption:=scanOption;
    scanner_.memscan.Roundingtype:=roundingtype;
    scanner_.memscan.Scanvalue1:=v1;
    scanner_.memscan.Scanvalue2:=v2;
    scanner_.memscan.Hexadecimal:=hexadecimal;
    scanner_.memscan.BinaryStringAsDecimal:=binaryStringAsDecimal;
    scanner_.memscan.Unicode:=unicode;
    scanner_.memscan.Casesensitive:=casesensitive;
    scanner_.memscan.Percentage:=percentage;
    scanner_.memscan.CompareToSavedScan:=compareToSavedScan;
    scanner_.memscan.SavedScanName:=BStrToCEUTF8String(savedscanname);
    scanner_.memscan.NextScan;
  end;
end;

function ICountAddressesFound():uint64; stdcall;
begin
  if assigned(scanner_) then
    result := scanner_.memscan.GetFoundCount
  else result := High(uint64);
end;

procedure IGetAddress(i: qword;out address: WideString; out value: WideString)stdcall;
var
  address_ : PtrUint;
  value_ : string;
  extra : dword;
begin
  address_:=0;
  value_:='';
  extra:=0;

  if assigned(scanner_) and assigned(scanner_.foundlist) and (i<scanner_.foundlist.count) then
  begin
    address_ := scanner_.foundlist.GetAddress(i,extra,value_);

    if address_=0 then
    begin
      address := 'null';
      value := 'null';
      exit;
    end;

    address := CEUTF8StringToBStr(IntToHex(address_,8));

    if (scanner_.foundlist.vartype=vtString) and (not scanner_.foundlist.isUnicode) then
      value := CEAnsiStringToBStr(value_)
    else
      value := CEUTF8StringToBStr(value_);
  end
  else begin
    address := 'null';
    value := 'null';
  end;

end;

procedure IResetValues;stdcall;
begin
  if (assigned(scanner_)) and (assigned(scanner_.foundlist)) then
  scanner_.foundlist.ResetValues;
end;

procedure IRebaseAddressList(index : integer);stdcall;
begin
  if (assigned(scanner_)) and assigned(scanner_.foundlist) then
    scanner_.foundlist.RebaseAddresslist(index);
end;

function IGetBinarySize():integer;stdcall;
begin
  if (assigned(scanner_)) and (assigned(scanner_.memscan)) then
     result := scanner_.memscan.Getbinarysize()
  else
  result := -1;
end;

procedure IConfigScanner(scanWritable: Tscanregionpreference;
  scanExecutable: Tscanregionpreference;scanCopyOnWrite: Tscanregionpreference);stdcall;
begin
  if (assigned(scanner_)) and (assigned(scanner_.memscan)) then
  begin
       scanner_.memscan.scanWritable:= scanWritable;
       scanner_.memscan.scanExecutable:= scanExecutable;
       scanner_.memscan.scanCopyOnWrite:= scanCopyOnWrite;
  end;
end;

end.

