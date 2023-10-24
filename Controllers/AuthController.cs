using System.Data;
using System.Security.Cryptography;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
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
                                        // Create passwordSalt by adding random number
                                        byte[] passwordSalt = new byte[128 / 8];
                                        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                                        {
                                                rng.GetNonZeroBytes(passwordSalt);
                                        }
                                        //to create passwordHash we need to add the passwordkey to passwordsalt to be more secure

                                        byte[] passwordHash = _authHelper.GetPasswordHash(userForRegitation.Password, passwordSalt);

                                        string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth([Email],
                                                [PasswordHash],
                                                [PasswordSalt]) VALUES('" + userForRegitation.Email +
                                                "',@PasswordHash,@PasswordSalt)";

                                        List<SqlParameter> sqlParameters = new();

                                        SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                                        passwordSaltParameter.Value = passwordSalt;

                                        SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                                        passwordHashParameter.Value = passwordHash;

                                        sqlParameters.Add(passwordSaltParameter);
                                        sqlParameters.Add(passwordHashParameter);

                                        if (_dapper.ExecuteSqlWithParameter(sqlAddAuth, sqlParameters))
                                        {
                                                string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users(
                                                        [FirstName],
                                                        [LastName],
                                                        [Email],
                                                        [Gender],
                                                        [Active]
                                                        ) VALUES(" +
                                                        "'" + userForRegitation.FirstName +
                                                        "','" + userForRegitation.LastName +
                                                        "','" + userForRegitation.Email +
                                                        "','" + userForRegitation.Gender +
                                                        "',1)";
                                                if (_dapper.ExecuteSql(sqlAddUser))
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

                [AllowAnonymous]
                [HttpPost("Login")]

                public IActionResult Login(UserForLoginDto userForLogin)
                {
                        string sqlHashAndSalt = @"SELECT
                                [PasswordHash],
                                [PasswordSalt] From TutorialAppSchema.Auth WHERE Email= '" + userForLogin.Email + "' ";

                        UserForLoginConfirmationDto userForConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlHashAndSalt);

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