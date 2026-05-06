namespace UserMicroservice.Models;

public class ApiResponseModel<TResult> where TResult : class
{
    public bool Status {get; set;}
    public string Message {get; set;} = string.Empty;

    public TResult? Response {get; set;}
}