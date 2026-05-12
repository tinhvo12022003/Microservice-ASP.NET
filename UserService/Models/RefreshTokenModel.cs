namespace UserMicroservice.Models;

public class RefreshTokenModel : BaseModel
{
    public Guid Id {get; set;}
    public string RefreshToken {get; set;} = null!;
    public DateTime ExpireTime {get; set;}
    // public string OldToken {get; set;} = string.Empty;

    public Guid UserId {get; set;}
    public UserModel? User {get; set;}
}