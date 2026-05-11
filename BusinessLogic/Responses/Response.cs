namespace social_media_console_app.BusinessLogic.Responses;

public class Response
{
    public bool    Success { get; set; } = true;
    public string? Message { get; set; }

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
