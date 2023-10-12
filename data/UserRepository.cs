using DotnetAPI.Models;

namespace DotnetAPI.Data
{
        public class UserRepository : IUserRepository
        {
                DataContextEF _entityFramework;

                public UserRepository(IConfiguration config)
                {
                        _entityFramework = new DataContextEF(config);
                }

                public bool SaveChanges()
                {
                        return _entityFramework.SaveChanges() > 0;
                }
                // public bool AddEntity<T>(T entityToAdd)
                public void AddEntity<T>(T entityToAdd)
                {
                        if (entityToAdd != null)
                        {
                                _entityFramework.Add(entityToAdd);
                                // return true;
                        }
                        // return false;
                }
                // public bool RemoveEntity<T>(T entityToAdd)
                public void RemoveEntity<T>(T entityToAdd)
                {
                        if (entityToAdd != null)
                        {
                                _entityFramework.Remove(entityToAdd);
                                // return true;
                        }
                        // return false;
                }

                public IEnumerable<User> GetUsers()
                {
                        IEnumerable<User> users = _entityFramework.Users.ToList<User>();
                        return users;
                }

                public User GetSingleUsers(int userId)
                {
                        User? user = _entityFramework.Users
                        .Where(u => u.UserId == userId)
                        .FirstOrDefault<User>();

                        if (user != null)
                        { return user; }
                        throw new Exception("Failed to Find User with UserId " + userId);
                }

                public UserSalary GetSingleUserSalary(int userId)
                {
                        UserSalary? UserSalary = _entityFramework.UserSalary
                        .Where(u => u.UserId == userId)
                        .FirstOrDefault<UserSalary>();

                        if (UserSalary != null)
                        { return UserSalary; }
                        throw new Exception("Failed to Find User with UserId " + userId);
                }

                public UserJobInfo GetSingleUserJobInfo(int userId)
                {
                        UserJobInfo? user = _entityFramework.UserJobInfo
                        .Where(u => u.UserId == userId)
                        .FirstOrDefault<UserJobInfo>();

                        if (user != null)
                        { return user; }
                        throw new Exception("Failed to Find User with UserId " + userId);
                }
        }
}