using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaporService.Helpers;
using VaporService.Models;
using VaporService.Storages;

namespace VaporService.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class JabberwockyController : ControllerBase
    {
        private readonly IStorage<string, Weapon> _weaponStorage;
        private readonly IStorage<string, Jabberwocky> _jabberwockyStorage;
        private readonly IFightForecaster _fightForecaster;

        public JabberwockyController(IStorage<string, Jabberwocky> jabberwockyStorage,
            IStorage<string, Weapon> weaponStorage, IFightForecaster fightForecaster)
        {
            _jabberwockyStorage = jabberwockyStorage;
            _weaponStorage = weaponStorage;
            _fightForecaster = fightForecaster;
        }

        [Authorize]
        [HttpPut]
        [Route("jabberwocky")]
        public async Task<IActionResult> PutJabberwocky(Jabberwocky jabberwocky)
        {
            await _jabberwockyStorage.Put(jabberwocky.BreedingSeed, jabberwocky);
            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("jabberwocky")]
        public async Task<IActionResult> GetJabberwocky(GetJabberwockyRequest request)
        {
            
            var jabberwocky = await _jabberwockyStorage.Get(request.BreedingSeed);
            if (jabberwocky == null)
                return NotFound();
            
            if (!jabberwocky.Breeder.Equals(User.Identity.Name))
                return StatusCode(403, "not your jabberwocky");

            return Ok(jabberwocky.ToJson());
        }

        [Authorize]
        [HttpPut]
        [Route("weaponTestResult")]
        public async Task<IActionResult> TestWeapon(TestWeaponRequest weaponRequest)
        {
            var jabberwocky = await _jabberwockyStorage.Get(weaponRequest.BreedingSeed);
            var weapon = await _weaponStorage.Get(weaponRequest.WeaponName);

            return Ok(_fightForecaster.Forecast(weapon, jabberwocky).ToJson());
        }

        [Authorize]
        [HttpGet]
        [Route("jabberwockyList")]
        public IActionResult GetJabberwockyList() => Ok(_jabberwockyStorage.GetKeys());

        public class GetJabberwockyRequest
        {
            public string BreedingSeed { get; set; }
        }

        public class TestWeaponRequest : GetJabberwockyRequest
        {
            public string WeaponName { get; set; }
        }
    }
}