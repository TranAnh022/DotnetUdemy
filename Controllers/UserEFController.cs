
using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]

public class UserEFController : ControllerBase
{
    DataContextEF _entityFramework;
    IUserRepository _userRepository;
    IMapper _mapper;

    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
        _entityFramework = new DataContextEF(config);
        _userRepository = userRepository;
        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserDto, User>();
            cfg.CreateMap<UserSalary, UserSalary>();
            cfg.CreateMap<UserJobInfo, UserJobInfo>();
        }));
    }

    // --- USER -- //
    [HttpGet("GetUsers")]

    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _userRepository.GetUsers();
        return users;
    }

    [HttpGet("GetSingleUsers/{userId}")]

    public User GetSingleUsers(int userId)
    {
        // User? user = _entityFramework.Users
        // .Where(u => u.UserId == userId)
        // .FirstOrDefault<User>();

        // if (user != null)
        // { return user; }
        // throw new Exception("Failed to Find User with UserId " + userId);
        return _userRepository.GetSingleUsers(userId);
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? userDB = _userRepository.GetSingleUsers(user.UserId);

        if (userDB != null)
        {
            userDB.Active = user.Active;
            userDB.FirstName = user.FirstName;
            userDB.LastName = user.LastName;
            userDB.Email = user.Email;
            userDB.Gender = user.Gender;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to Update User with UserId " + user.UserId);
        }

        throw new Exception("Failed to Find User with UserId " + user.UserId);
    }

    [HttpPost("AddUser")]

    public IActionResult AddUser(UserDto user)
    {
        User userDB = _mapper.Map < User>(user);

        _userRepository.AddEntity<User>(userDB);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to create User");
    }

    [HttpDelete("GetSingleUsers/{userId}")]

    public IActionResult DeleteUser(int userId)
    {
        User? userDB = _userRepository.GetSingleUsers(userId);

        if (userDB != null)
        {
            _userRepository.AddEntity<User>(userDB);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to Delete User with UserId " + userId);
        }

        throw new Exception("Failed to Find User with UserId " + userId);
    }

    //-- USER SALARY -- //
    [HttpGet("UserSalary/{userId}")]
    public UserSalary GetUserSalaryEF(int userId)
    {
        return _userRepository.GetSingleUserSalary(userId);
    }

    [HttpPost("UserSalary")]
    public IActionResult PostUserSalaryEf(UserSalary userForInsert)
    {
        _userRepository.AddEntity<UserSalary>(userForInsert);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Adding UserSalary failed on save");
    }


    [HttpPut("UserSalary")]
    public IActionResult PutUserSalaryEf(UserSalary userForUpdate)
    {
        UserSalary? userToUpdate = _userRepository.GetSingleUserSalary(userForUpdate.UserId);

        if (userToUpdate != null)
        {
            _mapper.Map(userForUpdate, userToUpdate);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Updating UserSalary failed on save");
        }
        throw new Exception("Failed to find UserSalary to Update");
    }


    [HttpDelete("UserSalary/{userId}")]
    public IActionResult DeleteUserSalaryEf(int userId)
    {
        UserSalary? userToDelete = _userRepository.GetSingleUserSalary(userId);

        if (userToDelete != null)
        {
            _userRepository.AddEntity<UserSalary>(userToDelete);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Deleting UserSalary failed on save");
        }
        throw new Exception("Failed to find UserSalary to delete");
    }

    // -- USER JOB INFO -- //

    [HttpGet("UserJobInfo/{userId}")]
    public UserJobInfo GetUserJobInfoEF(int userId)
    {
        return _userRepository.GetSingleUserJobInfo(userId);
    }

    [HttpPost("UserJobInfo")]
    public IActionResult PostUserJobInfoEf(UserJobInfo userForInsert)
    {
        _userRepository.AddEntity<UserJobInfo>(userForInsert);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("Adding UserJobInfo failed on save");
    }


    [HttpPut("UserJobInfo")]
    public IActionResult PutUserJobInfoEf(UserJobInfo userForUpdate)
    {
        UserJobInfo? userToUpdate = _userRepository.GetSingleUserJobInfo(userForUpdate.UserId);

        if (userToUpdate != null)
        {
            _mapper.Map(userForUpdate, userToUpdate);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Updating UserJobInfo failed on save");
        }
        throw new Exception("Failed to find UserJobInfo to Update");
    }


    [HttpDelete("UserJobInfo/{userId}")]
    public IActionResult DeleteUserJobInfoEf(int userId)
    {
        UserJobInfo? userToDelete = _userRepository.GetSingleUserJobInfo(userId);

        if (userToDelete != null)
        {
            _userRepository.RemoveEntity<UserJobInfo>(userToDelete);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Deleting UserJobInfo failed on save");
        }
        throw new Exception("Failed to find UserJobInfo to delete");
    }
}
