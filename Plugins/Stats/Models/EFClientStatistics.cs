﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedLibraryCore.Database.Models;

namespace IW4MAdmin.Plugins.Stats.Models
{
    public class EFClientStatistics : SharedEntity
    {
        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual EFClient Client { get; set; }
        public long ServerId { get; set; }
        [ForeignKey("ServerId")]
        public virtual EFServer Server { get; set; }
        [Required]
        public int Kills { get; set; }
        [Required]
        public int Deaths { get; set; }
        public double EloRating { get; set; }
        public virtual ICollection<EFHitLocationCount> HitLocations { get; set; }
        public double RollingWeightedKDR { get; set; }
        public double VisionAverage { get; set; }
        [NotMapped]
        public double Performance
        {
            get => Math.Round((EloRating + Skill) / 2.0, 2);
        }
        [NotMapped]
        public double KDR
        {
            get => Deaths == 0 ? Kills : Math.Round(Kills / (double)Deaths, 2);
        }
        [Required]
        public double SPM { get; set; }
        [Required]
        public double Skill { get; set; }
        [Required]
        public int TimePlayed { get; set; }
        [Required]
        public double MaxStrain { get; set; }

        [NotMapped]
        public float AverageHitOffset
        {
            get => (float)Math.Round(HitLocations.Sum(c => c.HitOffsetAverage) / Math.Max(1, HitLocations.Where(c => c.HitOffsetAverage > 0).Count()), 4);
        }
        [NotMapped]
        public int SessionKills { get; set; }
        [NotMapped]
        public int SessionDeaths { get; set; }
        [NotMapped]
        public int KillStreak { get; set; }
        [NotMapped]
        public int DeathStreak { get; set; }
        [NotMapped]
        public DateTime LastStatCalculation { get; set; }
        [NotMapped]
        public int LastScore { get; set; }
        [NotMapped]
        public DateTime LastActive { get; set; }
        [NotMapped]
        public double MaxSessionStrain { get; set; }
        public void StartNewSession()
        {
            KillStreak = 0;
            DeathStreak = 0;
            LastScore = 0;
            SessionScores.Add(0);
            Team = IW4Info.Team.None;
        }
        [NotMapped]
        public int SessionScore
        {
            set
            {
                SessionScores[SessionScores.Count - 1] = value;
            }
            get
            {
                return SessionScores.Sum();
            }
        }
        [NotMapped]
        public int RoundScore
        {
            get
            {
                return SessionScores[SessionScores.Count - 1];
            }
        }
        [NotMapped]
        private List<int> SessionScores = new List<int>() { 0 };
        [NotMapped]
        public IW4Info.Team Team { get; set; }
        [NotMapped]
        public DateTime LastStatHistoryUpdate { get; set; } = DateTime.UtcNow;
        [NotMapped]
        public double SessionSPM { get; set; }
    }
}
