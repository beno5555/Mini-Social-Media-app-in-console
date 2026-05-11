namespace social_media_console_app.BusinessLogic.Responses;

public class Response
{
    public bool    Success { get; set; } = true;
    public string? Message { get; set; }

    private Response(bool success, string? message = null)
    {
        Success = success;
        Message = message;
    }

    public Response()
    {
        
    }

    public static Response Ok()                 => new Response(true);
    public static Response Fail(string message) => new Response(false, message);
}
public class Response<T> : Response
{
    public T? Data { get; set; }
    
    private Response(bool success, string? message, T? data)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    public Response()
    {
        
    }

    public static     Response<T> Ok(T        data)    => new(true, null, data);
    public new static Response<T> Fail(string message) => new(false, message, default);
}
