using FirebaseApiMain.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirebaseApiMain.Infrastructure.Auth.Interface
{
    public interface IAuthInterface
    {

        Task<IActionResult> ManageAdminAsync(AdminUsers AdminUsersRequest);
    }
}
