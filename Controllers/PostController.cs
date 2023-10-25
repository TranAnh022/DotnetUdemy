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
                        if (postId != 0)
                        {
                                parameters += ", @PostId=" + postId.ToString();
                        }
                        if (userId != 0)
                        {
                                parameters += ", @UserId=" + userId.ToString();
                        }
                        if (searchParam.ToLower() != "none")
                        {
                                parameters += ", @SearchValue='" + searchParam + "'";
                        }

                        if (parameters.Length > 0)
                        {
                                sqlGetPosts += parameters[1..];
                                Console.WriteLine(sqlGetPosts);
                        }
                        return _dapper.LoadData<Post>(sqlGetPosts);
                }

                [HttpGet("MyPosts")]
                public IEnumerable<Post> GetMyPosts()
                {
                        string sqlGetPosts = @"EXEC TutorialAppSchema.spPosts_Get @UserId = " + User.FindFirst("userId")?.Value;

                        return _dapper.LoadData<Post>(sqlGetPosts);
                }

                [HttpPut("UpsertPost")]
                public IActionResult AddPost(Post post)
                {
                        string sql = @"EXEC TutorialAppSchema.spPosts_Upsert
                                @UserId= " + User.FindFirst("userId")?.Value
                                + ",@PostTitle='" + post.PostTitle
                                + "',@PostContent='" + post.PostContent + "'";

                        if (post.PostId > 0)
                        {
                                sql += ", @PostId = " + post.PostId;
                        }

                        if (_dapper.ExecuteSql(sql))
                        {
                                return Ok();
                        }
                        throw new Exception("Failed to create new post!");
                }


                [HttpDelete("Post/{postId}")]

                public IActionResult DeletePost(int postId)
                {
                        string sql = @"EXEC TutorialAppSchema.spPosts_Delete @PostId = " + postId.ToString() + ", @UserId = " + User.FindFirst("userId")?.Value;

                        if (_dapper.ExecuteSql(sql))
                        {
                                return Ok();
                        }
                        throw new Exception("Failed to delete post");
                }

        }

}