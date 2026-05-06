using AutoMapper;
using UserMicroservice.Config;
using UserMicroservice.Models;
using UserMicroservice.Repository;

namespace UserMicroservice.Service;

public class UserService
{

    private readonly UnitOfWork _unitOfWork;
    private readonly HashingConfig _hashing;
    private readonly IMapper _mapper;
    public UserService(UnitOfWork unitOfWork, HashingConfig hashing, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _hashing = hashing;
        _mapper = mapper;
    }

    public async Task<ApiResponseModel<UserViewModel>> Register(CreateUserRequestModel request)
    {
        try
        {
            if (
                string.IsNullOrEmpty(request.Fname) ||
                string.IsNullOrEmpty(request.Lname) ||
                string.IsNullOrEmpty(request.PasswordHash) ||
                string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Phone)
            )
            {
                return new ApiResponseModel<UserViewModel>
                {
                    Status = false,
                    Message = "Dữ liệu không được trống",
                    Response = null
                };
            }

            // check ton tai
            var isUserExisted = await _unitOfWork.userRepository.GetFilter(
                page: 1,
                limit: 1,
                filter: x => x.Email == request.Email || x.Phone == request.Phone
            );

            if (isUserExisted.TotalItems != 0) // nếu tồn tại
            {
                return new ApiResponseModel<UserViewModel>
                {
                    Status = false,
                    Message = "Người dùng đã tồn tại",
                    Response = null
                };
            }

            var Newuser = new UserModel
            {
                Fname = request.Fname,
                Lname = request.Lname,
                Email = request.Email,
                Phone = request.Phone,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                PasswordHash = _hashing.HashPassword(request.PasswordHash),
                Status = true
            };

            await _unitOfWork.userRepository.Add(Newuser);
            await _unitOfWork.CommitAsync();


            return new ApiResponseModel<UserViewModel>
            {
                Status = true,
                Message = "Success",
                Response = _mapper.Map<UserModel, UserViewModel>(Newuser)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Register Error]: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[Inner Error]: {ex.InnerException.Message}");
            }

            return new ApiResponseModel<UserViewModel>
            {
                Status = false,
                Message = "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.",
                Response = null
            };
        }
    }


}