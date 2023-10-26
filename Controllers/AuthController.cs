using System.Data;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DotnetAPI.Controllers
{
        [Authorize]
        [ApiController]
        [Route("[controller]")]
        public class AuthController : ControllerBase
        {
                private readonly DataContextDapper _dapper;

                private readonly AuthHelper _authHelper;

                private readonly ReusableSql _reusableSql;

                private readonly IMapper _mapper;

                public AuthController(IConfiguration config)
                {
                        _dapper = new DataContextDapper(config);

                        _authHelper = new AuthHelper(config);

                        _reusableSql = new ReusableSql(config);

                        _mapper = new Mapper(new MapperConfiguration(cfg =>
                        {
                                cfg.CreateMap<UserForRegistrationDto, UserComplete>();
                        })); //AutoMapper is used to map properties from one object to another when the property names and types match
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
                                                //AutoMapper helps streamline the process of transferring data between objects with similar property names and types.
                                                UserComplete userComplete = _mapper.Map<UserComplete>(userForRegitation);
                                                userComplete.Active = true;
                                                if (_reusableSql.UpsertUser(userComplete))
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