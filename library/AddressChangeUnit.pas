unit AddressChangeUnit;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, symbolhandler, byteinterpreter, CEFuncProc, LazUTF8;

procedure IProcessAddress(address : WideString ; vartype : TVariableType ; showashexadecimal: Boolean;
  showAsSigned: boolean; bytesize: Integer; out res_address : WideString);stdcall;

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

function CEUTF8StringToBStr(const value: string): WideString;
begin
  Result:=UTF8ToUTF16(value);
end;

function CEAnsiStringToBStr(const value: string): WideString;
begin
  Result:=UTF8ToUTF16(AnsiToUTF8(value));
end;

procedure IProcessAddress(address : WideString ; vartype : TVariableType ; showashexadecimal: Boolean;
  showAsSigned: boolean; bytesize: Integer; out res_address : WideString);stdcall;
var a: PtrUInt;
  e: boolean;
  s: string;
begin
  //read the address and display the value it points to

  a:=symhandler.getAddressFromName(BStrToCEAsciiOrAnsiString(address),false,e);
  if not e then
  begin
    //get the vartype and parse it
    s:=readAndParseAddress(a, vartype,nil,showashexadecimal, showAsSigned, bytesize);

    if vartype=vtString then
      res_address:=CEAnsiStringToBStr(s)
    else
      res_address:=CEUTF8StringToBStr(s);
  end
  else
    res_address:='???';
end;

end.

