namespace BeeQ.FileStorage.LocalDisk;

public class BasePathLocalDiskConfigratedException : System.Exception
{
    public BasePathLocalDiskConfigratedException() : base("Basepath is not configurated") { }
}
public class CreateIdLocalDiskConfigratedException : System.Exception
{
    public CreateIdLocalDiskConfigratedException() : base("CreateId is not configurated") { }
}
