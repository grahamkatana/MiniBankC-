using System;
using System.Linq;
using System.Threading.Tasks;
namespace MiniBank.Api.Dtos.Authentication;

public class NewUserDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;

}
