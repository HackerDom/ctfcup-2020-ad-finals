using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaporService.Storages;

namespace VaporService.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class JabberwockyController : ControllerBase
    {
        private readonly IStorage<string, Weapon> _weaponStorage;
        private readonly Random _random;
        private readonly IStorage<string, Jabberwocky> jabberwockyStorage;

        public JabberwockyController(IStorage<string, Jabberwocky> jabberwockyStorage,
            IStorage<string, Weapon> weaponStorage)
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
}