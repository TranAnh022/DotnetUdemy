using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers;


public class AuthHelper
{

        private readonly IConfiguration _config;

        private readonly DataContextDapper _dapper;

        public AuthHelper(IConfiguration config)
        {
                _dapper = new DataContextDapper(config);

                _config = config;
        }

        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
                string passwordSaltPlusString = _config.GetSection("AppSetting:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

                return KeyDerivation.Pbkdf2(
                        password: password,
                        salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 100000,
                        numBytesRequested: 256 / 8
                );
        }

        public string CreateToken(int userId)
        {
                Claim[] claims = new Claim[]
                {
                        new("userId",userId.ToString())
                };

                string? tokenKeyString = _config.GetSection("Appsettings:TokenKey").Value;

                SymmetricSecurityKey tokenKey = new(Encoding.UTF8.GetBytes(tokenKeyString != null ? tokenKeyString : ""));

                SigningCredentials credentials = new(tokenKey, SecurityAlgorithms.HmacSha256Signature);

                SecurityTokenDescriptor desciptor = new()
                {
                        Subject = new ClaimsIdentity(claims),
                        SigningCredentials = credentials,
                        Expires = DateTime.Now.AddDays(1)
                };
                JwtSecurityTokenHandler tokenHandler = new();

                SecurityToken token = tokenHandler.CreateToken(desciptor);

                return tokenHandler.WriteToken(token);
        }

        public bool SetPassword(UserForLoginDto userForSetPassword)
        {
                // Create passwordSalt by adding random number
                byte[] passwordSalt = new byte[128 / 8];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                        rng.GetNonZeroBytes(passwordSalt);
                }
                //to create passwordHash we need to add the passwordkey to passwordsalt to be more secure

                byte[] passwordHash = GetPasswordHash(userForSetPassword.Password, passwordSalt);

                string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert
                                               @Email= @EmailParam,
                                               @PasswordHash = @PasswordHash,
                                               @PasswordSalt = @PasswordSalt";

                DynamicParameters sqlParameters = new();

                sqlParameters.Add("@EmailParam", userForSetPassword.Email, DbType.String);
                sqlParameters.Add("@PasswordHash", passwordHash, DbType.Binary);
                sqlParameters.Add("@PasswordSalt", passwordSalt, DbType.Binary);

                //------------Another way -------------------
                // List<SqlParameter> sqlParameters = new();

                // SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
                // emailParameter.Value = userForSetPassword.Email;
                // sqlParameters.Add(emailParameter);

                // SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                // passwordSaltParameter.Value = passwordSalt;
                // sqlParameters.Add(passwordSaltParameter);

                // SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                // passwordHashParameter.Value = passwordHash;
                //sqlParameters.Add(passwordHashParameter);



                return _dapper.ExecuteSqlWithParameter(sqlAddAuth, sqlParameters);

        }
}

