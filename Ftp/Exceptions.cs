namespace BeeQ.FileStorage.Ftp;

public class FtpUrlConfigratedException : System.Exception
{
    public FtpUrlConfigratedException() : base("Url is not configurated") { }
}
