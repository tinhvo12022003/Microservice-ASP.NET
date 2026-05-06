using UserMicroservice.Models;
using UserMicroservice.Repository;

namespace UserMicroservice.Service;

public class UserService
{

    private readonly UnitOfWork _unitOfWork;
    public UserService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserModel> Register(UserDTO user)
    {
        if (
            string.IsNullOrEmpty(user.Fname) ||
            string.IsNullOrEmpty(user.Lname) ||
            string.IsNullOrEmpty(user.PasswordHash) ||
            string.IsNullOrEmpty(user.Email) ||
            string.IsNullOrEmpty(user.Phone)
        )
            throw new Exception("Thong tin khong duoc trong");

        // check ton tai
        var isUserExisted = await _unitOfWork.userRepository.GetFilter(
            page: 1,
            limit: 1,
            filter: x => x.Email == user.Email || x.Phone == user.Phone
        );

        if (isUserExisted.TotalItems != 0)
        {
            throw new Exception("Email hoac so dien thoai da ton tai");
        }

        var Newuser = new UserModel
        {
            Fname = user.Fname,
            Lname = user.Lname,
            Email = user.Email,
            Phone = user.Phone, 
            BirthDate = user.BirthDate,
            Gender = user.Gender,
            PasswordHash = user.PasswordHash,
            Role = 1,
            Status = true  
        };

        await _unitOfWork.userRepository.Add(Newuser);
        await _unitOfWork.CommitAsync();

        return Newuser;
    }
}