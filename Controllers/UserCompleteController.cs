
using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;

    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _reusableSql= new ReusableSql(config);
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

        string stringParameters = "";

        DynamicParameters sqlParameters = new();


        if (userId != 0)
        {
            stringParameters += ",@UserId=@UserIdParameter";
            sqlParameters.Add("@UserIdParameter", userId,DbType.Int32);
        }

        if (isActive)
        {
            stringParameters += ",@Active =@ActiveParameter";
            sqlParameters.Add("@ActiveParameter", isActive, DbType.Boolean);
        }
        if (stringParameters.Length > 0)
        {
            sql += stringParameters[1..];//, parameters.Length); // using rage operator
        }

        IEnumerable<UserComplete> users = _dapper.LoadDataWithParameter<UserComplete>(sql,sqlParameters);
        return users;
    }

    // ----------------------- UPSERT ENDPOINT -----------//
    [HttpPut("UpSertUser")]
    public IActionResult UpSertUser(UserComplete user)
    {
        if (_reusableSql.UpsertUser(user))
        {
            return Ok();
        }

        throw new Exception("Failed to Update User");
    }
    // ----------------------- DELETE ENDPOINT -----------//

    [HttpDelete("User/{userId}")]

    public IActionResult DeleteUser(int userId)
    {
        string sql = @"TutorialAppSchema.spUser_Delete @UserId = @UserIdParameter";

        DynamicParameters sqlParameters = new();
        sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);

        if (_dapper.ExecuteSqlWithParameter(sql, sqlParameters))
        {
            return Ok();
        }
        throw new Exception("Failed to Delete User");
    }

}
