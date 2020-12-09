using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        //TODO: add list

        [Authorize]
        [HttpPut]
        [Route("claimed/weapon")]
        public async Task<IActionResult> PutClaimedWeapon(Weapon weapon)
        {
            if (!_claimedWeaponIndex.ClaimWeapon(User?.Identity?.Name, weapon.Name))
                return Forbid("Already claimed");

            _claimedWeaponIndex.ClaimWeapon(weapon.Name, User?.Identity?.Name);
            await _weaponStorage.Put(weapon.Name, weapon);

            return Ok(await _weaponStorage.Get(weapon.Name));
        }

        [HttpPut]
        [Route("shared/weapon")]
        public async Task<IActionResult> PutSharedWeapon(Weapon weapon)
        {
            await _weaponStorage.Put(weapon.Name, weapon);
            return Ok(_weaponStorage.Get(weapon.Name));
        }

        [HttpGet]
        [Route("weapon")]
        public async Task<IActionResult> GetWeapon(GetWeaponRequest request)
        {
            if (_claimedWeaponIndex.IsClaimed(request.WeaponName) &&
                !_claimedWeaponIndex.IsOwner(User?.Identity?.Name, request.WeaponName))
                return Forbid();

            return Ok(await _weaponStorage.Get(request.WeaponName));
        }

        public class GetWeaponRequest
        {
            public string WeaponName { get; set; }
        }
    }
}