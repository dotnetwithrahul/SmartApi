using FirebaseApiMain.Infrastructure.Auth.Interface;
using FirebaseApiMain.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FirebaseApiMain.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IAuthInterface _IAuthInterface;
        public LoginController(IAuthInterface IAuthInterface)
        {
            _IAuthInterface = IAuthInterface;
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login(AdminUsers AdminUsersRequest)
        {
            AdminUsersRequest.Flag = "login";
            return await _IAuthInterface.ManageAdminAsync(AdminUsersRequest);
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(AdminUsers AdminUsersRequest)
        {
            AdminUsersRequest.Flag = "create";
            return await _IAuthInterface.ManageAdminAsync(AdminUsersRequest);
        }
    }
}
