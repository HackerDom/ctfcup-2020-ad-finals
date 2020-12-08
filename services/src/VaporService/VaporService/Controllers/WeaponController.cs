using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaporService.Storages;

namespace VaporService.Controllers
{
    public class Jabberwocky
    {
       public string BreedingSeed { get; set; }
       public string ArcaneProperty { get; set; }
       public bool IsHasJawsThatBite { get; set; }
       public bool IsHasClawsThatCatch { get; set; }
       
    }
    
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class JabberwockyController : ControllerBase
    {
        private IStorage<string, Jabberwocky> jabberwockyStorage;
        private readonly IStorage<string, Weapon> _weaponStorage;
        private Random _random;

        public JabberwockyController(IStorage<string, Jabberwocky> jabberwockyStorage, IStorage<string, Weapon> weaponStorage)
        {
            this.jabberwockyStorage = jabberwockyStorage;
            _weaponStorage = weaponStorage;
            _random = new Random();
        }

        [Authorize]
        public async Task<IActionResult> PutJabberwocky(Jabberwocky jabberwocky)
        {
            await jabberwockyStorage.Put(jabberwocky.BreedingSeed, jabberwocky);
            return Ok();
        }
        
        [Authorize]
        public async Task<IActionResult> GetJabberwocky(GetJabberwockyRequest request)
        {
            var result = await jabberwockyStorage.Get(request.BreedingSeed);
            if (result == null)
                return NotFound();
            
            return Ok(result);
        }

        [Authorize]
        public async Task<IActionResult> CanDefeatJabberwocky(DefateTestRequest request)
        {
            var result = await jabberwockyStorage.Get(request.BreedingSeed);
            var weapon = await _weaponStorage.Get(request.WeaponName);
            if (weapon.IsVorpal && _random.Next(0, 100) < 5)
                return Ok(true);

            if (weapon.Property.Contains("sudo"))
                return Ok(true);

            if (string.Compare(weapon.ArcaneProperty, result.ArcaneProperty, StringComparison.Ordinal) < 0)
                return Ok(false);

            if (result.IsHasClawsThatCatch ^ result.IsHasJawsThatBite && _random.Next(0, 100) < 50)
                return Ok(true);
            
            if (result.IsHasClawsThatCatch && result.IsHasJawsThatBite)
                return Ok(false);

            return Ok(true);
        }

        public class GetJabberwockyRequest
        {
            public string BreedingSeed { get; set; }
        }
        
        public class DefateTestRequest : GetJabberwockyRequest
        {
            public string WeaponName { get; set; }
        }
        
        //TODO: add list
    }
    
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