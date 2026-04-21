unit PEInfounit;

{$MODE Delphi}

interface

uses
  Windows;

function peinfo_getcodesize(header: pointer; headersize: integer=0): dword;
function peinfo_getentryPoint(header: pointer; headersize: integer=0): ptrUint;
function peinfo_getcodebase(header: pointer; headersize: integer=0): ptrUint;
function peinfo_getdatabase(header: pointer; headersize: integer=0): ptrUint;
function peinfo_getheadersize(header: pointer): dword;

implementation

function peinfo_getImageDosHeader(headerbase: pointer): PImageDosHeader;
begin
  Result:=nil;
  if headerbase=nil then exit;

  if PImageDosHeader(headerbase)^.e_magic<>IMAGE_DOS_SIGNATURE then exit;
  Result:=headerbase;
end;

function peinfo_getImageNtHeaders(headerbase: pointer; headersize: integer=0): PImageNtHeaders;
var
  ImageDosHeader: PImageDosHeader;
begin
  Result:=nil;
  ImageDosHeader:=peinfo_getImageDosHeader(headerbase);
  if ImageDosHeader=nil then exit;

  if (ImageDosHeader^._lfanew<0) then exit;
  if (headersize<>0) and (ImageDosHeader^._lfanew>(headersize-sizeof(TImageNtHeaders))) then exit;

  Result:=PImageNtHeaders(PByte(headerbase)+ImageDosHeader^._lfanew);
  if Result^.Signature<>IMAGE_NT_SIGNATURE then
    Result:=nil;
end;

function peinfo_getcodesize(header: pointer; headersize: integer=0): dword;
var
  ImageNTHeader: PImageNtHeaders;
begin
  Result:=0;

  ImageNTHeader:=peinfo_getImageNtHeaders(header, headersize);
  if ImageNTHeader=nil then exit;

  Result:=ImageNTHeader.OptionalHeader.SizeOfCode;
end;

function peinfo_getdatabase(header: pointer; headersize: integer=0): ptrUint;
var
  ImageNTHeader: PImageNtHeaders;
begin
  Result:=0;

  ImageNTHeader:=peinfo_getImageNtHeaders(header, headersize);
  if ImageNTHeader=nil then exit;
  if ImageNTHeader.FileHeader.Machine=$8664 then exit;

  Result:=PImageNtHeaders32(ImageNTHeader).OptionalHeader.BaseOfData;
end;

function peinfo_getcodebase(header: pointer; headersize: integer=0): ptrUint;
var
  ImageNTHeader: PImageNtHeaders;
begin
  Result:=0;

  ImageNTHeader:=peinfo_getImageNtHeaders(header, headersize);
  if ImageNTHeader=nil then exit;

  Result:=ImageNTHeader.OptionalHeader.BaseOfCode;
end;

function peinfo_getEntryPoint(header: pointer; headersize: integer=0): ptrUint;
var
  ImageNTHeader: PImageNtHeaders;
begin
  Result:=0;

  ImageNTHeader:=peinfo_getImageNtHeaders(header, headersize);
  if ImageNTHeader=nil then exit;

  Result:=ImageNTHeader.OptionalHeader.AddressOfEntryPoint;
end;

function peinfo_getheadersize(header: pointer): dword;
var
  ImageNTHeader: PImageNtHeaders;
begin
  Result:=0;

  ImageNTHeader:=peinfo_getImageNtHeaders(header, $1000);
  if ImageNTHeader=nil then exit;

  Result:=ImageNTHeader.OptionalHeader.SizeOfHeaders;
end;

end.