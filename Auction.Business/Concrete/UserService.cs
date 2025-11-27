using Auction.Business.Abstraction;
using Auction.Business.Dtos;
using Auction.Core.Models;
using Auction.DataAccess.Context;
using Auction.DataAccess.Enums;
using Auction.DataAccess.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auction.Business.Concrete
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ApiResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretkey;
        public UserService(ApplicationDbContext context, IMapper mapper, ApiResponse response, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration _configuration)
        {
            _context = context;
            _mapper = mapper;
            _response = response;
            _userManager = userManager;
            _roleManager = roleManager;
            secretkey = _configuration["SecretKey:jwtkey"];
        }

        public async Task<ApiResponse> Login(LoginRequestDto model)
        {
            ApplicationUser userFromDb = _context.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
            if (userFromDb != null)
            {
                bool isValid = await _userManager.CheckPasswordAsync(userFromDb, model.Password);
                if (!isValid)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Your entry information is not correct");
                    _response.isSuccess = false;
                    return _response;
                }
                var role = await _userManager.GetRolesAsync(userFromDb);
                JwtSecurityTokenHandler tokenHandler = new();
                byte[] key = Encoding.ASCII.GetBytes(secretkey);

                SecurityTokenDescriptor tokenDescriptor = new()
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier,userFromDb.Id),
                        new Claim(ClaimTypes.Email,userFromDb.Email),
                        new Claim(ClaimTypes.Role,role.FirstOrDefault()),
                        new Claim("fullname",userFromDb.Id),
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                LoginResponseModel _model = new()
                {
                    Email = userFromDb.Email,
                    Token = tokenHandler.WriteToken(token)
                };

                _response.Result = _model;
                _response.isSuccess = true;
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return _response;
            }
            _response.isSuccess = false;
            _response.ErrorMessages.Add("Username or password is incorrect");
            return _response;
        }

        public async Task<ApiResponse> Register(RegisterRequestDto model)
        {
            var userFromDb = _context.ApplicationUsers.FirstOrDefault(x => x.UserName.ToLower() == model.UserName.ToLower());
            if (userFromDb != null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.isSuccess = false;
                _response.ErrorMessages.Add("Username already exist");
                return _response;
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                Email = model.UserName,
                UserName = model.UserName,
                FullName = model.FullName,
                ProfilePicture ="a"
            };
            //var newUser = _mapper.Map<ApplicationUser>(model);
            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                var isTrue = _roleManager.RoleExistsAsync(UserType.Administrator.ToString()).GetAwaiter().GetResult();
                if (!_roleManager.RoleExistsAsync(UserType.Administrator.ToString()).GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserType.Administrator.ToString()));
                    await _roleManager.CreateAsync(new IdentityRole(UserType.Seller.ToString()));
                    await _roleManager.CreateAsync(new IdentityRole(UserType.NormalUser.ToString()));
                }
                if (model.UserType.ToString().ToLower() == UserType.Administrator.ToString().ToLower())
                {
                    await _userManager.AddToRoleAsync(newUser, UserType.Administrator.ToString());
                }
                else if (model.UserType.ToString().ToLower() == UserType.Seller.ToString().ToLower())
                {
                    await _userManager.AddToRoleAsync(newUser, UserType.Seller.ToString());
                }
                else if(model.UserType.ToString().ToLower() == UserType.NormalUser.ToString().ToLower())
                {
                    await _userManager.AddToRoleAsync(newUser, UserType.NormalUser.ToString());
                }
                _response.StatusCode = System.Net.HttpStatusCode.Created;
                _response.isSuccess = true;
                return _response;
            }
            foreach (var error in result.Errors)
            {
                _response.ErrorMessages.Add(error.ToString());
            }
            return _response;
        }
    }
}
