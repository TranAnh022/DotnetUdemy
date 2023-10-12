using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Controllers
{
        public class AuthController : ControllerBase
        {
                private readonly DataContextDapper _dapper;

                private readonly IConfiguration _config;

                public AuthController(IConfiguration config)
                {
                        _dapper = new DataContextDapper(config);
                        _config = config;
                }

                [HttpPost("Register")]

                public IActionResult Register(UserForRegistrationDto userForRegitation)
                {
                        if(userForRegitation.Password == userForRegitation.PasswordConfirm)
                        {
                                string sqlCheckUserExists = @"SELECT [Email] From TutorialAppSchema.Auth WHERE Email= '" + userForRegitation.Email + "'";

                                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                                if(existingUsers.Count()== 0)
                                {
                                        byte[] passwordSalt = new byte[128 / 8];
                                        using(RandomNumberGenerator rng = RandomNumberGenerator.Create())
                                        {
                                                rng.GetNonZeroBytes(passwordSalt);
                                        }

                                        string passwordSaltPlusString = _config.GetSection("AppSetting:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

                                        byte[] passwordHash = KeyDerivation.Pbkdf2(
                                                password: userForRegitation.Password,
                                                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                                                prf:KeyDerivationPrf.HMACSHA256,
                                                iterationCount:100000,
                                                numBytesRequested:256/8
                                        );

                                        string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth([Email],
                                                [PasswordHash],
                                                [PasswordSalt]) VALUES('" + userForRegitation.Email +
                                                "',@PasswordHash,@PasswordSalt)";

                                        List<SqlParameter> sqlParameters = new();

                                        SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt",SqlDbType.VarBinary);
                                        passwordSaltParameter.Value = passwordSalt;

                                        SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                                        passwordHashParameter.Value = passwordHash;

                                        sqlParameters.Add(passwordSaltParameter);
                                        sqlParameters.Add(passwordHashParameter);

                                        return Ok();
                                }
                                throw new Exception("User with this email already exists!");

                        }
                        throw new Exception("Password does not match!");
                }


                [HttpPost("Login")]

                public IActionResult Login(UserForLoginDto userForLogin)
                {
                        return Ok();
                }

        }
}