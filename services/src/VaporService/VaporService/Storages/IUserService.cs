using System.Collections.Generic;
using System.Threading.Tasks;
using VaporService.Models;

namespace VaporService.Storages
{
    public interface IUserService
    {
        Task<UserData> Authenticate(string username, string password);
        Task<IEnumerable<UserData>> GetAll();
    }
}