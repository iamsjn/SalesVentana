using SalesVentana.Data;
using SalesVentana.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SalesVentana.Services
{
    public class MembershipService : IMembershipService
    {
        #region Variables
        private readonly IBaseRepository<User> _userRepository;
        private readonly IBaseRepository<UserRole> _userRoleRepository;
        private readonly IBaseRepository<Role> _roleRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        public MembershipService(IBaseRepository<User> userRepository, IBaseRepository<UserRole> userRoleRepository,
            IBaseRepository<Role> roleRepository, IEncryptionService encryptionService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _encryptionService = encryptionService;
            _unitOfWork = unitOfWork;
        }

        private void addUserToRole(User user, int roleId)
        {
            var role = _roleRepository.GetSingle<RoleMapper>("Sales_Role", "RoleId", roleId.ToString());

            if (role == null)
                throw new ApplicationException("Role doesn't exist.");
            var userRole = new UserRole()
            {
                RoleId = role.ID,
                UserId = user.ID
            };

            Dictionary<string, object> columnValue = new Dictionary<string, object>();
            columnValue.Add("RoleId", userRole.RoleId);
            columnValue.Add("UserId", userRole.UserId);

            _userRoleRepository.Add("Sales_UserRole", columnValue);
        }

        private bool isPasswordValid(User user, string password)
        {
            return string.Equals(_encryptionService.EncryptPassword(password,
           user.Salt), user.HashedPassword);
        }

        private bool isUserValid(User user, string password)
        {
            if (isPasswordValid(user, password))
            {
                return !user.IsLocked;
            }
            return false;
        }

        public User CreateUser(string username, string email, string password, int[] roles)
        {
            var existingUser = _userRepository.GetSingleByUsername(username);
            if (existingUser != null)
            {
                throw new Exception("Username is already in use");
            }
            var passwordSalt = _encryptionService.CreateSalt();
            var user = new User()
            {
                Username = username,
                Salt = passwordSalt,
                Email = email,
                IsLocked = false,
                HashedPassword = _encryptionService.EncryptPassword(password, passwordSalt),
                DateCreated = DateTime.Now
            };

            Dictionary<string, object> columnValue = new Dictionary<string, object>();
            columnValue.Add("Username", user.Username);
            columnValue.Add("Email", user.Email);
            columnValue.Add("HashedPassword", user.HashedPassword);
            columnValue.Add("Salt", user.Salt);
            columnValue.Add("IsLocked", user.IsLocked);
            columnValue.Add("DateCreated", user.DateCreated);

            _userRepository.Add("Sales_User", columnValue);

            if (roles != null || roles.Length > 0)
            {
                foreach (var role in roles)
                {
                    addUserToRole(user, role);
                }
            }

            _unitOfWork.Terminate();
            return user;
        }

        public User GetUser(int userId)
        {
            //return _userRepository.GetSingle(userId);
            return new User();
        }
        public List<Role> GetUserRoles(string username)
        {
            List<Role> _result = new List<Role>();
            var existingUser = _userRepository.GetSingleByUsername(username);

            if (existingUser != null)
            {
                existingUser.UserRoles = _userRoleRepository.FindBy<UserRoleMapper>("Sales_UserRole", "UserId", existingUser.ID.ToString());
                foreach (var userRole in existingUser.UserRoles)
                {
                    userRole.Role = _roleRepository.FindBy<RoleMapper>("Sales_Role", "ID", userRole.RoleId.ToString()).FirstOrDefault();
                    _result.Add(userRole.Role);
                }
            }

            return _result.Distinct().ToList();
        }
        public MembershipContext ValidateUser(string username, string password)
        {
            var membershipCtx = new MembershipContext();
            var user = _userRepository.GetSingleByUsername(username);
            if (user != null && isUserValid(user, password))
            {
                var userRoles = GetUserRoles(user.Username);
                membershipCtx.User = user;

                var identity = new GenericIdentity(user.Username);
                membershipCtx.Principal = new GenericPrincipal(
                identity,
               userRoles.Select(x => x.Name).ToArray());
            }

            _unitOfWork.Terminate();
            return membershipCtx;
        }
    }
}