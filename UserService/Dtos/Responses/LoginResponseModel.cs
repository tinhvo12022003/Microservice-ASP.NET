namespace UserMicroservice.Models;

public class LoginResponseModel
{
    public UserViewModel? UserView {get; set;}
    public TokenResponseModel? Tokens {get; set;}
}