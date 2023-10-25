
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    DataContextDapper _dapper;

    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }
    // ----------------------- TEST CONNECTION -----------//
    [HttpGet("TestConnection")]

    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    // ----------------------- GET ENDPOINT -----------//
    [HttpGet("GetUsers/{userId}/{isActive}")]

    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get ";

        string parameters = "";

        if (userId != 0)
        {
            parameters += ",@UserId =" + userId.ToString();
        }

        if (isActive)
        {
            parameters += ",@Active =" + isActive.ToString();
        }
        if (parameters.Length > 0)
        {
            sql += parameters[1..];//, parameters.Length); // using rage operator
            Console.WriteLine(sql);
        }

        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
        return users;
    }

    // ----------------------- UPSERT ENDPOINT -----------//
    [HttpPut("UpSertUser")]
    public IActionResult EditUser(UserComplete user)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_UpSert
            @FirstName = '" + user.FirstName +
            "', @LastName = '" + user.LastName +
            "', @Email = '" + user.Email +
            "', @Gender = '" + user.Gender +
            "', @Active = '" + user.Active +
            "', @JobTitle= '" + user.JobTitle +
            "', @Department = '" + user.Department +
            "', @Salary = '" + user.Salary +
            "', @UserId = " + user.UserId;

        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to Update User");
    }
    // ----------------------- DELETE ENDPOINT -----------//

    [HttpDelete("User/{userId}")]

    public IActionResult DeleteUser(int userId)
    {
        string sql = @"TutorialAppSchema.spUser_Delete @UserId = " + userId.ToString();

        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Delete User");
    }

}
