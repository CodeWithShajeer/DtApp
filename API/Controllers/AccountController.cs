
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService )
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")] //api/account/register
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
        {
            if( await UserExists(registerDto.username))
                return BadRequest("username taken");

           using var hmac = new HMACSHA512();
           var user = new AppUser
           {
            UserName=registerDto.username.ToLower(),
            PasswordHas=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
            PasswordSalt=hmac.Key
           };
           _context.Users.Add(user);
           await _context.SaveChangesAsync();
           return new UserDTO{
            Username=user.UserName,
            Token=_tokenService.CreateToken(user)
           };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x=>x.UserName==username.ToLower());
        }

        [HttpPost("login")] //api/account/login
        public async Task<ActionResult<UserDTO>> LogIn(LogInDTO loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x=>x.UserName==loginDto.username);

            if(user==null)
                return Unauthorized("Invalid Username");
            
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i=0; i<computedHash.Length; i++)
            {
                    if(computedHash[i]!=user.PasswordHas[i])
                        return Unauthorized ("Inavalid password");
            }
            
            return new UserDTO
            {
                Username=user.UserName,
                Token=_tokenService.CreateToken(user)
            };
        }

    }
}