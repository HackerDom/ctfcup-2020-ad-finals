using VaporService.Models;

namespace VaporService
{
    public interface IFightForecaster
    {
        Report Forecast(Weapon weapon, Jabberwocky jabberwocky);
    }
}