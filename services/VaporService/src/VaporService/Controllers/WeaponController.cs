using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaporService.Helpers;
using VaporService.Models;
using VaporService.Storages;

namespace VaporService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeaponController : ControllerBase
    {
        private readonly IClaimesIndex _claimesIndex;
        private readonly IStorage<string, Weapon> _weaponStorage;

        public WeaponController(IClaimesIndex claimesIndex, IStorage<string, Weapon> weaponStorage)
        {
            _claimesIndex = claimesIndex;
            _weaponStorage = weaponStorage;
        }

        [Authorize]
        [HttpGet]
        [Route("weaponList")]
        public IActionResult GetWeaponList() => Ok(_weaponStorage.GetKeys().ToJson());

        [Authorize]
        [HttpPut]
        [Route("claimed")]
        public async Task<IActionResult> PutClaimedWeapon(Weapon weapon)
        {
            if (!_claimesIndex.ClaimWeapon(weapon.Name, User?.Identity?.Name))
                return Conflict("Already claimed");

            await _weaponStorage.Put(weapon.Name, weapon);

            return Ok();
        }

        [HttpPut]
        [Route("shared")]
        public async Task<IActionResult> PutSharedWeapon(Weapon weapon)
        {
            await _weaponStorage.Put(weapon.Name, weapon);
            return Ok();
        }

        [HttpPost]
        [Route("weapon")]
        public async Task<IActionResult> GetWeapon(GetWeaponRequest request)
        {
            if (_claimesIndex.IsClaimed(request.WeaponName) &&
                !_claimesIndex.IsOwner(User?.Identity?.Name, request.WeaponName))
                return StatusCode(403);

            var weapon = await _weaponStorage.Get(request.WeaponName);
            return Ok(weapon.ToJson());
        }

        public class GetWeaponRequest
        {
            public string WeaponName { get; set; }
        }
    }
}