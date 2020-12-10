using VaporService.Storages;

namespace VaporService.Controllers
{
    public interface IFightForecaster
    {
        Report Forecast(Weapon weapon, Jabberwocky jabberwocky);
    }
}