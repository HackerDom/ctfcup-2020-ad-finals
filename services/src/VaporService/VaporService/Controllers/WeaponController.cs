using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaporService.Helpers;
using VaporService.Storages;

namespace VaporService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeaponController : ControllerBase
    {
        private readonly IClaimedWeaponIndex _claimedWeaponIndex;
        private readonly IStorage<string, Weapon> _weaponStorage;

        public WeaponController(IClaimedWeaponIndex claimedWeaponIndex, IStorage<string, Weapon> weaponStorage)
        {
            _claimedWeaponIndex = claimedWeaponIndex;
            _weaponStorage = weaponStorage;
        }

        [Authorize]
        [HttpGet]
        [Route("weaponList")]
        public IActionResult GetWeaponList() => Ok(_weaponStorage.GetKeys().ToJson());

        [Authorize]
        [HttpPut]
        [Route("claimed/weapon")]
        public async Task<IActionResult> PutClaimedWeapon(Weapon weapon)
        {
            if (!_claimedWeaponIndex.ClaimWeapon(User?.Identity?.Name, weapon.Name))
                return Forbid("Already claimed");

            _claimedWeaponIndex.ClaimWeapon(weapon.Name, User?.Identity?.Name);
            await _weaponStorage.Put(weapon.Name, weapon);

            return Ok();
        }

        [HttpPut]
        [Route("shared/weapon")]
        public async Task<IActionResult> PutSharedWeapon(Weapon weapon)
        {
            await _weaponStorage.Put(weapon.Name, weapon);
            return Ok();
        }

        [HttpGet]
        [Route("weapon")]
        public async Task<IActionResult> GetWeapon(GetWeaponRequest request)
        {
            if (_claimedWeaponIndex.IsClaimed(request.WeaponName) &&
                !_claimedWeaponIndex.IsOwner(User?.Identity?.Name, request.WeaponName))
                return Forbid();

            var weapon = await _weaponStorage.Get(request.WeaponName);
            return Ok(weapon.ToJson());
        }

        public class GetWeaponRequest
        {
            public string WeaponName { get; set; }
        }
    }
}