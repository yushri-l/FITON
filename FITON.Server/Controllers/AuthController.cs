using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FITON.Server.Utils.Database;
using FITON.Server.Models;

namespace FITON.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IJwtService _jwt;

        public 
    }
}
