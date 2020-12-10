using System;
using VaporService.Storages;
using Vostok.Logging.Abstractions;

namespace VaporService.Controllers
{
    class FightForecaster : IFightForecaster
    {
        private readonly ILog _log;
        private readonly Random _random;

        public FightForecaster(ILog log)
        {
            _log = log;
            _random = new Random();
        }

        public Report Forecast(Weapon weapon, Jabberwocky jabberwocky)
        {

            if (weapon.IsVorpal && _random.Next(0, 100) < 5)
                return new Report
                {
                    Reason = "Beheaded",
                    JabberwockyDefeated = true
                };

            if (weapon.Property.Contains("sudo"))
                return new Report
                {
                    Reason = "cheat",
                    JabberwockyDefeated = true
                };

            if (weapon.ArcaneProperty.CompareTo(jabberwocky.ArcaneProperty) < 0)
                return new Report
                {
                    Reason = "Weak arcane property",
                    JabberwockyDefeated = true
                };

            if (jabberwocky.HasClawsThatCatch ^ jabberwocky.HasJawsThatBite && _random.Next(0, 100) < 50)
                return new Report
                {
                    Reason = "Luck",
                    JabberwockyDefeated = true
                };
            ;

            if (jabberwocky.HasClawsThatCatch && jabberwocky.HasJawsThatBite)
                return new Report
                {
                    Reason = "KO. Jabberwocky wins",
                    JabberwockyDefeated = true
                };
            ;

            return new Report
            {
                Reason = "Good always triumphs over evil",
                JabberwockyDefeated = true
            };
        }
    }
}