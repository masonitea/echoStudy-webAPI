﻿using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using echoStudy_webAPI.Areas.Identity.Data;

namespace echoStudy_webAPI.Data
{
    public interface IJwtAuthenticationManager
    {
        string Authenticate(EchoUser user);
    }

    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        private readonly string _key;

        public JwtAuthenticationManager(string key)
        {
            _key = key;
        }

        public string Authenticate(EchoUser user)
        {
            // need a security token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            // need byte array of key
            var tokenKey = Encoding.ASCII.GetBytes(_key);
            // per docs: This is a place holder for all the attributes related to the issued token.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // per docs: Gets or sets the output claims to be included in the issued token.
                // TODO: This probably needs to be changed. Check what the resulting Jwt looks like!
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("sub", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email) // we can add more to the JWT payload here
                }),
                // TEST VALUE. Change token expiry window for staging/prod
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
