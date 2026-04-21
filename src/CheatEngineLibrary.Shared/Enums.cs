namespace CheatEngine;

public enum TScanOption
{
    UnknownValue = 0,
    ExactValue = 1,
    ValueBetween = 2,
    BiggerThan = 3,
    SmallerThan = 4,
    IncreasedValue = 5,
    IncreasedValueBy = 6,
    DecreasedValue = 7,
    DecreasedValueBy = 8,
    Changed = 9,
    Unchanged = 10,
    Custom,
};

public enum TScanType
{
    NewScan,
    FirstScan,
    NextScan,
};

public enum TRoundingType
{
    Rounded = 0,
    Extremerounded = 1,
    Truncated = 2,
};

public enum TVariableType
{
    Byte = 0,
    Word = 1,
    Dword = 2,
    Qword = 3,
    Single = 4,
    Double = 5,
    String = 6,
    UnicodeString = 7,
    ByteArray = 8,
    Binary = 9,
    All = 10,
    AutoAssembler = 11,
    Pointer = 12,
    Custom = 13,
    Grouped = 14,
    ByteArrays = 15,
}; // all ,grouped and MultiByteArray are special types

public enum TCustomScanType
{
    None,
    AutoAssembler,
    CPP,
    DLLFunction,
};

public enum TFastScanMethod
{
    NotAligned = 0,
    Aligned = 1,
    LastDigits = 2,
};

public enum TScanRegionPreference
{
    DontCare,
    Exclude,
    Include,
};