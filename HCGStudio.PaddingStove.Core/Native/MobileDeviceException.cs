namespace HCGStudio.PaddingStove.Core.Native;

public class MobileDeviceException<T> : Exception
{
    private readonly T _status;
    internal MobileDeviceException(T status)
    {
        _status = status;
    }
    public override string Message => $"Error occurred while calling native API: {_status}";
}

