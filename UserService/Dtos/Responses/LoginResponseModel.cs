namespace UserMicroservice.Models;

public class LoginResponseModel
{
    public UserViewModel? UserView {get; set;}
    public string AccessToken {get; set;} = string.Empty;
}