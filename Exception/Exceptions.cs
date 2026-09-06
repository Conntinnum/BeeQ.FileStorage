namespace BeeQ.FileStorage.Exception;

public class UploadInterceptorConfigratedException : System.Exception
{
    public UploadInterceptorConfigratedException() : base("No one OnUpload's method are configurated") { }
}

public class GetInterceptorConfigratedException : System.Exception
{
    public GetInterceptorConfigratedException() : base("No one OnGet's methods are configurated") { }
}

public class GetFilenameInterceptorConfigratedException : System.Exception
{
    public GetFilenameInterceptorConfigratedException() : base("OnGetFilename is not configurated") { }
}

public class CreateIdInterceptorConfigratedException : System.Exception
{
    public CreateIdInterceptorConfigratedException() : base("CreateId is not configurated") { }
}

public class DeleteInterceptorConfigratedException : System.Exception
{
    public DeleteInterceptorConfigratedException() : base("Delete is not configurated") { }
}

public class FileNotFoundException : System.Exception
{
    public FileNotFoundException() : base("File not found") { }
}