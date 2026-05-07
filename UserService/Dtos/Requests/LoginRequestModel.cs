using System.Text.Json.Serialization;

namespace UserMicroservice.Models;

public class LoginRequestModel
{
    public string Email {get; set;} = string.Empty;
    
    public string PlainPassword {get; set;} = string.Empty;
}