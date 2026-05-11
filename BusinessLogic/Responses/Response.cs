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

    // public static Response Ok()                 => new Response(true);
    // public static Response Fail(string message) => new Response(false, message);

    public void Ok()  => Success = true; 
    public virtual void Fail(string message)
    {
        Success = false;
        Message = message;
    }
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

    public void Ok(T data)
    {
        Success = true;
        Data = data;
    }

    public override void Fail(string message)
    {
        Success = false;
        Message = message;
        Data = default;
    }
}
