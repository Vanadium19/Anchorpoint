using System;
using UnityEngine;

namespace WeaponModule
{
    [Serializable]
    public class WeaponWearSettings
    {
        [Tooltip("Wear percent (0-100) added by one shot. Set it per weapon.")]
        [Min(0f)]
        [SerializeField] private float wearPerShot = 0.4f;
        [Tooltip("Multiplier M of the wear per shot. Reserved for weapon modifications.")]
        [Min(0f)]
        [SerializeField] private float wearMultiplier = 1f;
        [Tooltip("Wear percent below which the weapon never malfunctions.")]
        [Range(0f, 100f)]
        [SerializeField] private float malfunctionThreshold = 30f;
        [Tooltip("Malfunction chance percent at 100% wear. The chance grows linearly from 0% at the threshold.")]
        [Range(0f, 100f)]
        [SerializeField] private float maxMalfunctionChance = 10f;
        [Min(0f)]
        [SerializeField] private float misfireWeight = 70f;
        [Min(0f)]
        [SerializeField] private float jamWeight = 30f;
        [Tooltip("Seconds needed to clear a misfire.")]
        [Min(0f)]
        [SerializeField] private float misfireClearTime = 0.5f;
        [Tooltip("Seconds added to the regular reload time to clear a jam.")]
        [Min(0f)]
        [SerializeField] private float jamClearTimeBonus = 1.5f;
        [Tooltip("Rounds lost from the magazine when a jam is cleared.")]
        [Min(0)]
        [SerializeField] private int jamLostRounds = 1;

        public float WearPerShot => wearPerShot;
        public float WearMultiplier => wearMultiplier;
        public float MalfunctionThreshold => malfunctionThreshold;
        public float MaxMalfunctionChance => maxMalfunctionChance;
        public float MisfireWeight => misfireWeight;
        public float JamWeight => jamWeight;
        public float MisfireClearTime => misfireClearTime;
        public float JamClearTimeBonus => jamClearTimeBonus;
        public int JamLostRounds => jamLostRounds;
    }
}
