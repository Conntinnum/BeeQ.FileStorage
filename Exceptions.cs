namespace BeeQ;

public partial class FileStorage
{
    public class UploadInterceptorConfigratedException : Exception
    {
        public UploadInterceptorConfigratedException() : base("No one OnUpload's method are configurated") { }
    }
    public class GetInterceptorConfigratedException : Exception
    {
        public GetInterceptorConfigratedException() : base("No one OnGet's methods are configurated") { }
    }
    public class GetFileIdInterceptorConfigratedException : Exception
    {
        public GetFileIdInterceptorConfigratedException() : base("OnGetFileId is not configurated") { }
    }
    public class GetFilenameInterceptorConfigratedException : Exception
    {
        public GetFilenameInterceptorConfigratedException() : base("OnGetFilename is not configurated") { }
    }
}