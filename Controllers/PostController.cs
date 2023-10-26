using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
        [Authorize]
        [ApiController]
        [Route("[controller]")]

        public class PostController : ControllerBase
        {
                private readonly DataContextDapper _dapper;

                public PostController(IConfiguration config)
                {
                        _dapper = new DataContextDapper(config);

                }

                [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
                public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
                {
                        string sqlGetPosts = @"EXEC TutorialAppSchema.spPosts_Get";
                        string parameters = "";
                        DynamicParameters sqlParameters = new();

                        if (postId != 0)
                        {
                                parameters += ", @PostId=@PostIdParameter";
                                sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);
                        }
                        if (userId != 0)
                        {
                                parameters += ", @UserId=@UserIdParameter";
                                sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
                        }
                        if (searchParam.ToLower() != "none")
                        {
                                parameters += ", @SearchValue=@SearchValueParameter";
                                sqlParameters.Add("@SearchValueParameter", searchParam, DbType.String);
                        }

                        if (parameters.Length > 0)
                        {
                                sqlGetPosts += parameters[1..];
                        }
                        return _dapper.LoadDataWithParameter<Post>(sqlGetPosts,sqlParameters);
                }

                [HttpGet("MyPosts")]
                public IEnumerable<Post> GetMyPosts()
                {
                        string sqlGetPosts = @"EXEC TutorialAppSchema.spPosts_Get @UserId =@UserIdParameter ";

                        DynamicParameters sqlParameters = new();

                        sqlParameters.Add("@UserIdParameter", User.FindFirst("userId")?.Value, DbType.Int32);

                        return _dapper.LoadData<Post>(sqlGetPosts);
                }

                [HttpPut("UpsertPost")]
                public IActionResult AddPost(Post post)
                {
                        string sql = @"EXEC TutorialAppSchema.spPosts_Upsert
                                @UserId= @UserIdParameter,
                                @PostTitle=@PostTitleParameter,
                                @PostContent=@PostContentParameter";

                        DynamicParameters sqlParameters = new();

                        sqlParameters.Add("@UserIdParameter", User.FindFirst("userId")?.Value, DbType.Int32);
                        sqlParameters.Add("@PostTitleParameter", post.PostTitle, DbType.String);
                        sqlParameters.Add("@PostContentParameter", post.PostContent, DbType.String);
                        if (post.PostId > 0)
                        {
                                sql += ", @PostId = @PostIdParameter";
                                sqlParameters.Add("@PostIdParameter", post.PostId, DbType.Int32);
                        }

                        if (_dapper.ExecuteSqlWithParameter(sql,sqlParameters))
                        {
                                return Ok();
                        }
                        throw new Exception("Failed to create new post!");
                }


                [HttpDelete("Post/{postId}")]

                public IActionResult DeletePost(int postId)
                {
                        string sql = @"EXEC TutorialAppSchema.spPosts_Delete
                        @PostId = @PostIdParameter,
                        @UserId = @UserIdParameter";
                        
                        DynamicParameters sqlParameters = new();

                        sqlParameters.Add("@UserIdParameter", User.FindFirst("userId")?.Value, DbType.Int32);
                        sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);

                        if (_dapper.ExecuteSqlWithParameter(sql,sqlParameters))
                        {
                                return Ok();
                        }
                        throw new Exception("Failed to delete post");
                }

        }

}