using System;
using VaporService.Helpers;
using VaporService.Models;

namespace VaporService
{
    class FightForecaster : IFightForecaster
    {
        private readonly Random _random;

        public FightForecaster()
        {
            _random = new Random();
        }

        
        public Report Forecast(Weapon weapon, Jabberwocky jabberwocky)
        {
            if (weapon.IsVorpal)
                return CheckForVorpalWeapon(jabberwocky);

            if (weapon.Force > jabberwocky.Force)
                return new Report()
                {
                    Reason = "OK. Fighter wins",
                    JabberwockyDefeated = true
                };

            if (weapon.Force < jabberwocky.Force)
                return new Report()
                {
                    Reason = "KO. Jabberwocky wins",
                    JabberwockyDefeated = false
                };

            if (Math.Abs(weapon.Force - jabberwocky.Force) < Double.Epsilon)
                return new Report()
                {
                    Reason = "OK. Fighter wins, but not sure",
                    JabberwockyDefeated = true
                };


            throw new ArgumentException("Unpredictable jabberwocky or weapon", $"{jabberwocky} {weapon}");
        }

        private Report CheckForVorpalWeapon(Jabberwocky jabberwocky)
        {
            if (_random.Next(0, 100) < 5)
            {
                return new Report
                {
                    Reason = "Beheaded",
                    JabberwockyDefeated = true
                };
            }

            if (!jabberwocky.HasClawsThatCatch ^ jabberwocky.HasJawsThatBite && _random.Next(0, 100) < 50)
                return new Report
                {
                    Reason = "Luck",
                    JabberwockyDefeated = true
                };

            return new Report
            {
                Reason = "KO. Jabberwocky wins",
                JabberwockyDefeated = false
            };
        }
    }
}