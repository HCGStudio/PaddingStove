using System.Runtime.InteropServices;

namespace HCGStudio.PaddingStove.Core.Native.External;

internal enum DeviceConnectionType
{
    Usb = 1,
    Network
}
    
[StructLayout(LayoutKind.Sequential)]
internal readonly unsafe struct DeviceInfo
{
    public readonly nint Udid;
    public readonly DeviceConnectionType ConnectionType;
    public readonly nint ConnectionData;
}

internal enum DeviceErrorStatus
{
    Success = 0,
    InvalidArguments = -1,
    UnknownError = -2,
    NoDevice = -3,
    NotEnoughData = -4,
    SslError = -6,
    Timeout = -7
}

[Flags]
internal enum DeviceOptions
{
    LookupUsb = 1 << 1,
    LookupNetwork = 1 << 2,
    LookupPreferNetwork = 1 << 3
}

internal enum LockdownErrorStatus
{
    // Custom errors
    Success = 0,
    InvalidArgument = -1,
    InvalidConfiguration = -2,
    PlistError = -3,
    PairingFailed = -4,
    SslError = -5,
    DictionaryError = -6,
    ReceiveTimeout = -7,
    MuxError = -8,
    NoRunningSession = -9,

    // Native errors
    InvalidResponse = -10,
    MissingKey = -11,
    MissingValue = -12,
    GetProhibited = -13,
    SetProhibited = -14,
    RemoveProhibited = -15,
    ImmutableValue = -16,
    PasswordProtected = -17,
    UserDeniedPairing = -18,
    PairingDialogResponsePending = -19,
    MissingHostId = -20,
    InvalidHostId = -21,
    SessionActive = -22,
    SessionInactive = -23,
    MissingSessionId = -24,
    InvalidSessionId = -25,
    MissingService = -26,
    InvalidService = -27,
    ServiceLimit = -28,
    MissingPairRecord = -29,
    SavePairRecordFailed = -30,
    InvalidPairRecord = -31,
    InvalidActivationRecord = -32,
    MissingActivationRecord = -33,
    ServiceProhibited = -34,
    EscrowLocked = -35,
    PairingProhibitedOverThisConnection = -36,
    FmipProtected = -37,
    McProtected = -38,
    McChallengeRequired = -39,
    UnknownError = -256
}

internal enum LockdownServiceErrorStatus
{
    Success = 0,
    InvalidArgument = -1,
    MuxError = -3,
    SslError = -4,
    StartServiceError = -5,
    NotEnoughData = -6,
    Timeout = -7,
    UnknownError = -256
}

internal enum HouseArrestErrorStatus
{
    Success = 0,
    InvalidArgument = -1,
    PlistError = -2,
    ConnectionFailed = -3,
    InvalidMode = -4,
    UnknownError = -256
}

internal enum AfcErrorStatus
{
    Success = 0,
    UnknownError = 1,
    OperationHeaderInvalid = 2,
    NoResources = 3,
    ReadError = 4,
    WriteError = 5,
    UnknownPacketType = 6,
    InvalidArgument = 7,
    ObjectNotFound = 8,
    ObjectIsDirectory = 9,
    PermissionDenied = 10,
    ServiceNotConnected = 11,
    OperationTimeout = 12,
    TooMuchData = 13,
    EndOfData = 14,
    OperationNotSupported = 15,
    ObjectExists = 16,
    ObjectBusy = 17,
    NoSpaceLeft = 18,
    OperationWouldBlock = 19,
    IoError = 20,
    OperationInterrupted = 21,
    OperationInProgress = 22,
    InternalError = 23,
    MuxError = 30,
    NoMemory = 31,
    NotEnoughData = 32,
    DirectoryNotEmpty = 33
}
