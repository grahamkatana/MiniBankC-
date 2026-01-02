using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniBank.Api.Models;

namespace MiniBank.Api.Interfaces
{
    public interface ITokenService
    {
         public string CreateToken(AppUser user);
        
    }
}