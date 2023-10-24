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

                [HttpGet("Posts")]
                public IEnumerable<Post> GetPosts()
                {
                        string sqlGetPosts = @"SELECT
                                [PostId],
                                [UserId],
                                [PostTitle],
                                [PostContent],
                                [PostCreated],
                                [PostUpdated] FROM TutorialAppSchema.Posts";
                        return _dapper.LoadData<Post>(sqlGetPosts);
                }

                [HttpGet("PostSingle/{postId}")]
                public Post GetPost(int postId)
                {
                        string sqlGetPost = @"SELECT
                                [PostId],
                                [UserId],
                                [PostTitle],
                                [PostContent],
                                [PostCreated],
                                [PostUpdated] FROM TutorialAppSchema.Posts WHERE PostId = '" + postId.ToString() + "'";
                        return _dapper.LoadDataSingle<Post>(sqlGetPost);
                }

                [HttpGet("PostsByUser/{userId}")]
                public IEnumerable<Post> GetPostsByUser(int userId)
                {
                        string sqlGetPosts = @"SELECT
                                [PostId],
                                [UserId],
                                [PostTitle],
                                [PostContent],
                                [PostCreated],
                                [PostUpdated] FROM TutorialAppSchema.Posts WHERE UserId = '" + userId.ToString() + "'";
                        return _dapper.LoadData<Post>(sqlGetPosts);
                }

                [HttpGet("MyPosts")]
                public IEnumerable<Post> GetMyPosts()
                {
                        string sqlGetPosts = @"SELECT
                                [PostId],
                                [UserId],
                                [PostTitle],
                                [PostContent],
                                [PostCreated],
                                [PostUpdated] FROM TutorialAppSchema.Posts WHERE UserId = " + User.FindFirst("userId")?.Value;


                        return _dapper.LoadData<Post>(sqlGetPosts);
                }

                [HttpPost("Post")]
                public IActionResult AddPost(PostToAddDto post)
                {
                        string sql = @"
                        INSERT INTO TutorialAppSchema.Posts(
                                [UserId],
                                [PostTitle],
                                [PostContent],
                                [PostCreated],
                                [PostUpdated]
                        ) VALUES ('" + User.FindFirst("userId")?.Value
                        + "','" + post.PostTitle
                        + "','" + post.PostContent
                        + "',GETDATE(),GETDATE())";


                        if (_dapper.ExecuteSql(sql))
                        {
                                return Ok();
                        }
                        throw new Exception("Failed to create new post!");
                }

                [HttpPut("Post")]
                public IActionResult EditPost(PostToEditDto post)
                {
                        string sql = @"
                         UPDATE TutorialAppSchema.Posts SET
                                PostTitle = '" + post.PostTitle +
                                "',PostContent = '" + post.PostContent +
                                "',PostUpdated = GETDATE() WHERE PostId = " + post.PostId.ToString() +
                                " AND UserId = " + User.FindFirst("userId")?.Value;

                        Console.WriteLine(sql);
                        if (_dapper.ExecuteSql(sql))
                        {
                                return Ok();
                        }
                        throw new Exception("Failed to edit post!");
                }

                [HttpDelete("Post/{postId}")]

                public IActionResult DeletePost(int postId)
                {
                        string sql = @"DELETE FROM TutorialAppSchema.Posts WHERE PostId = " + postId.ToString() + "AND UserId = " + User.FindFirst("userId")?.Value;

                        Console.WriteLine(sql);

                        if (_dapper.ExecuteSql(sql))
                        {
                                return Ok();
                        }
                        throw new Exception("Failed to delete post");
                }

                [HttpGet("PostBySearch/{searchParam}")]
                public IEnumerable<Post> PostBySearch(string searchParam)
                {
                        string sqlGetPosts = @"SELECT
                                [PostId],
                                [UserId],
                                [PostTitle],
                                [PostContent],
                                [PostCreated],
                                [PostUpdated] FROM TutorialAppSchema.Posts WHERE PostTitle LIKE '%" + searchParam + " %'" +
                                "OR PostContent LIKE '%" + searchParam + "%'";


                        return _dapper.LoadData<Post>(sqlGetPosts);
                }

        }
}