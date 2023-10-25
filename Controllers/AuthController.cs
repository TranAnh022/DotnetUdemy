using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;



namespace DotnetAPI.Controllers
{
        [Authorize]
        [ApiController]
        [Route("[controller]")]
        public class AuthController : ControllerBase
        {
                private readonly DataContextDapper _dapper;

                private readonly AuthHelper _authHelper;

                public AuthController(IConfiguration config)
                {
                        _dapper = new DataContextDapper(config);

                        _authHelper = new AuthHelper(config);
                }

                [AllowAnonymous]
                [HttpPost("Register")]

                public IActionResult Register(UserForRegistrationDto userForRegitation)
                {
                        if (userForRegitation.Password == userForRegitation.PasswordConfirm)
                        {
                                string sqlCheckUserExists = @"SELECT [Email] From TutorialAppSchema.Auth WHERE Email= '" + userForRegitation.Email + "'";

                                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                                if (existingUsers.Count() == 0)
                                {
                                        UserForLoginDto userForSetPassword = new()
                                        {
                                                Email = userForRegitation.Email,
                                                Password = userForRegitation.Password
                                        };

                                        if (_authHelper.SetPassword(userForSetPassword))
                                        {
                                                string sql = @"EXEC TutorialAppSchema.spUser_Upsert
                                                        @FirstName = '" + userForRegitation.FirstName +
                                                        "', @LastName = '" + userForRegitation.LastName +
                                                        "', @Email = '" + userForRegitation.Email +
                                                        "', @Gender = '" + userForRegitation.Gender +
                                                        "', @Active = 1" +
                                                        ", @JobTitle= '" + userForRegitation.JobTitle +
                                                        "', @Department = '" + userForRegitation.Department +
                                                        "', @Salary =" + userForRegitation.Salary;

                                                Console.WriteLine(sql);

                                                if (_dapper.ExecuteSql(sql))
                                                {
                                                        return Ok();
                                                }
                                                throw new Exception("Fail to add user.");
                                        }
                                        throw new Exception("Fail to register user.");
                                }
                                throw new Exception("User with this email already exists!");
                        }
                        throw new Exception("Password does not match!");
                }


                [HttpPut("ResetPassword")]

                public IActionResult ResetPassword(UserForLoginDto userForSetPassword)
                {
                        if (_authHelper.SetPassword(userForSetPassword))
                        {
                                return Ok();
                        }
                        throw new Exception("Fail to change the password!");
                }


                [AllowAnonymous]
                [HttpPost("Login")]

                public IActionResult Login(UserForLoginDto userForLogin)
                {
                        string sqlHashAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation_Get
                                @Email = @EmailParam";

                        DynamicParameters sqlParameters = new();

                        // SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
                        // emailParameter.Value = userForLogin.Email;
                        // sqlParameters.Add(emailParameter);
                        sqlParameters.Add("@EmailParam",userForLogin.Email,DbType.String);

                        UserForLoginConfirmationDto userForConfirmation = _dapper.LoadDataSingleWithParameter<UserForLoginConfirmationDto>(sqlHashAndSalt, sqlParameters);

                        byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

                        // if(passwordHash == userForConfirmation.PasswordHash) //Won't work because userForConfirmation.Password is an object therefore it never be exactly equal
                        //If we want to compare we need to compare the pointer (address) in the memorize

                        for (int index = 0; index < passwordHash.Length; index++)
                        {
                                if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                                {
                                        return StatusCode(401, "Incorrect Password!");
                                }
                        }

                        string userIdSql = @"SELECT [UserId] FROM TutorialAppSchema.Users WHERE Email= '" + userForLogin.Email + "' ";

                        int userId = _dapper.LoadDataSingle<int>(userIdSql);

                        return Ok(new Dictionary<string, string>{
                                {"token",_authHelper.CreateToken(userId)}
                        });
                }


                [HttpGet("RefreshToken")]
                public string RefreshToken()

                {
                        //FindFirst method retrieves the first claim with the specified claim type. 
                        string sqlGetUserId = @"SELECT [UserId] FROM TutorialAppSchema.Users WHERE UserId= '" + User.FindFirst("userId")?.Value + "' ";

                        int userId = _dapper.LoadDataSingle<int>(sqlGetUserId);

                        return _authHelper.CreateToken(userId);
                }

        }
}