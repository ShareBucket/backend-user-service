using AutoMapper;
using ShareBucket.DataAccessLayer.Data;
using ShareBucket.DataAccessLayer.Models.Entities;
using ShareBucket.UserMicroService.Models.Users;
using BCryptNet = BCrypt.Net.BCrypt;
using ShareBucket.UserMicroService.Helpers;
using ShareBucket.JwtMiddlewareClient.Services;
using UserMicroService.Models.Users;

namespace ShareBucket.UserMicroService.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<User> GetAll();
        User GetById(int id);
        void Register(RegisterRequest model);
        void Update(User user, UpdateRequest model);
        void Delete(User user);
        bool AddFriend(User user, string userEmail);
        ICollection<UserDataResponse> GetFriends(User user);
        bool IsFriend(User user, string userEmail);
        bool RemoveFriend(User user, string userEmail);
    }

    public class UserService : IUserService
    {
        private DataContext _context;
        private IJwtUtils _jwtUtils;
        private readonly IMapper _mapper;

        public UserService(
            DataContext context,
            IJwtUtils jwtUtils,
            IMapper mapper)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _mapper = mapper;
        }
        
        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Email == model.Email);

            // validate
            if (user == null || !BCryptNet.Verify(model.Password, user.PasswordHash))
                throw new AppException("Email or password is incorrect");

            //// authentication successful
            var response = _mapper.Map<AuthenticateResponse>(user);
            response.Token = _jwtUtils.GenerateToken(user.Id);
            return response;
        }

        public IEnumerable<User> GetAll()
        {
            var x = _jwtUtils.ValidateToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEiLCJuYmYiOjE2ODI2MjkzMDksImV4cCI6MTY4MzIzNDEwOSwiaWF0IjoxNjgyNjI5MzA5fQ.ggAW-4eMDRGY8MEs9SlfMSbChi5t4Lw4fbJX3IFOC1M");

            x = 2;
            return _context.Users;
        }

        public User GetById(int id)
        {
            return getUser(id);
        }

        public void Register(RegisterRequest model)
        {
            // validate
            if (_context.Users.Any(x => x.Email == model.Email))
                throw new AppException("Email '" + model.Email + "' is already taken");

            // map model to new user object
            var user = _mapper.Map<User>(model);

            // hash password
            user.PasswordHash = BCryptNet.HashPassword(model.Password);

            // save user
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(User user, UpdateRequest model)
        {

            // validate
            if (model.Email != user.Email && _context.Users.Any(x => x.Email == model.Email))
                throw new AppException("Username '" + model.Email + "' is already taken");

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = BCryptNet.HashPassword(model.Password);

            // copy model to user and save
            _mapper.Map(model, user);
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(User user)
        {
            // Remove all the friendships that the user is involved in
            var friendships = _context.Friendships.Where(f => f.UserId == user.Id || f.FriendId == user.Id);
            _context.Friendships.RemoveRange(friendships);

            // Remove all the memoryArea that the user is owner
            // Select all the memoryAreas that the user is owner
            var memoryAreas = _context.MemoryAreas.Where(m => m.UserOwnerId == user.Id);


            // Remove all the folder of the memoryAreas
            foreach (var memoryArea in memoryAreas)
            {
                // Remove all the metadatas of the memoryAreas
                var metadatas = _context.Metadatas.Where(m => m.MemoryAreaId == memoryArea.Id);
                _context.Metadatas.RemoveRange(metadatas);

                string localFilePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"UploadedFiles/{memoryArea.Id}"));

                if (Directory.Exists(localFilePath))
                {
                    Directory.Delete(localFilePath, true);
                }
            }

            _context.MemoryAreas.RemoveRange(memoryAreas);

            // Remove the user
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        // helper methods

        private User getUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
        }

        public bool AddFriend(User user, string userEmail)
        {
            var friend = _context.Users.SingleOrDefault(u => u.Email == userEmail);
            if (friend == null)
            {
                return false;
            }
            
            var userEntry = _context.Entry(user);
            userEntry.Collection(u => u.Friendships).Load();

            _context.Friendships.Add(new Friendship { UserId = user.Id, FriendId = friend.Id });
            _context.SaveChanges();
            return true;
        }

        public ICollection<UserDataResponse> GetFriends(User user)
        {
            var userEntry = _context.Entry(user);
            userEntry.Collection(u => u.Friendships).Load();
            
            // Load only the friend data (not the user data)
            var friends = _context.Friendships
                .Where(f => f.UserId == user.Id || f.FriendId == user.Id)
                .Select(f => f.UserId == user.Id ? f.Friend : f.User)
                .Select(u => new UserDataResponse
                { 
                    Id = u.Id, 
                    Email = u.Email, 
                    FirstName = u.FirstName, 
                    LastName = u.LastName 
                })
                .ToList();

            // Select the friend data
            
            


            return friends;
        }

        public bool IsFriend(User user, string userEmail)
        {
            var userEntry = _context.Entry(user);
            userEntry.Collection(u => u.Friendships).Load();

            var friend = _context.Users.SingleOrDefault(u => u.Email == userEmail);
            if (friend == null)
            {
                return false;
            }

            return user.Friendships.Any(f => f.FriendId == friend.Id || f.UserId == friend.Id);
        }

        public bool RemoveFriend(User user, string userEmail)
        {
            var userEntry = _context.Entry(user);
            userEntry.Collection(u => u.Friendships).Load();

            var friend = _context.Users.SingleOrDefault(u => u.Email == userEmail);
            if (friend == null)
            {
                return false;
            }

            var friendship = user.Friendships.SingleOrDefault(f => f.FriendId == friend.Id || f.UserId == friend.Id);
            if (friendship == null)
            {
                return false;
            }

            _context.Friendships.Remove(friendship);
            _context.SaveChanges();
            return true;
        }
    }
}