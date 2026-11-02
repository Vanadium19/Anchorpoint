using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace WeaponModule.Tests
{
    public class WeaponStatComposerTests
    {
        private const float Tolerance = 0.0001f;

        private static WeaponStats CreateBaseStats() => new(20f, 30f, 2f, 0.8f);

        private static WeaponStatModifier Add(WeaponStat stat, float value) =>
            new(stat, WeaponStatOperation.Add, value);

        private static WeaponStatModifier Multiply(WeaponStat stat, float value) =>
            new(stat, WeaponStatOperation.Multiply, value);

        [Test]
        public void Compose_WithoutModifiers_ReturnsBaseStats()
        {
            var stats = WeaponStatComposer.Compose(CreateBaseStats(), Array.Empty<WeaponStatModifier>());

            Assert.AreEqual(20f, stats.Damage, Tolerance);
            Assert.AreEqual(30, stats.MagazineCapacity);
            Assert.AreEqual(2f, stats.HipSpreadAngle, Tolerance);
            Assert.AreEqual(0.8f, stats.AimSpreadAngle, Tolerance);
        }

        [Test]
        public void Compose_AddAndMultiply_AppliesFormula()
        {
            var modifiers = new[]
            {
                Add(WeaponStat.Damage, 10f),
                Multiply(WeaponStat.Damage, 0.5f),
                Multiply(WeaponStat.Damage, 0.2f)
            };

            var stats = WeaponStatComposer.Compose(CreateBaseStats(), modifiers);

            // (20 + 10) * (1 + 0.5) * (1 + 0.2)
            Assert.AreEqual(54f, stats.Damage, Tolerance);
        }

        [Test]
        public void Compose_ModifiersOfOtherStats_DoNotAffectEachOther()
        {
            var modifiers = new[]
            {
                Multiply(WeaponStat.AimSpreadAngle, -0.6f),
                Add(WeaponStat.MagazineCapacity, 15f)
            };

            var stats = WeaponStatComposer.Compose(CreateBaseStats(), modifiers);

            Assert.AreEqual(20f, stats.Damage, Tolerance);
            Assert.AreEqual(45, stats.MagazineCapacity);
            Assert.AreEqual(2f, stats.HipSpreadAngle, Tolerance);
            Assert.AreEqual(0.32f, stats.AimSpreadAngle, Tolerance);
        }

        [Test]
        public void Compose_ModifierOrder_DoesNotChangeResult()
        {
            var modifiers = new List<WeaponStatModifier>
            {
                Multiply(WeaponStat.Damage, 0.25f),
                Add(WeaponStat.Damage, 5f),
                Multiply(WeaponStat.Damage, -0.1f),
                Add(WeaponStat.Damage, -2f),
                Multiply(WeaponStat.HipSpreadAngle, -0.2f),
                Add(WeaponStat.MagazineCapacity, 15f)
            };

            var expected = WeaponStatComposer.Compose(CreateBaseStats(), modifiers);

            for (var shift = 1; shift < modifiers.Count; shift++)
            {
                var shifted = new List<WeaponStatModifier>(modifiers);
                shifted.RemoveRange(0, shift);
                shifted.AddRange(modifiers.GetRange(0, shift));

                AssertEqual(expected, WeaponStatComposer.Compose(CreateBaseStats(), shifted));
            }

            var reversed = new List<WeaponStatModifier>(modifiers);
            reversed.Reverse();

            AssertEqual(expected, WeaponStatComposer.Compose(CreateBaseStats(), reversed));
        }

        [Test]
        public void Compose_NegativeResult_IsFlooredAtZero()
        {
            var modifiers = new[]
            {
                Add(WeaponStat.Damage, -100f),
                Multiply(WeaponStat.HipSpreadAngle, -2f)
            };

            var stats = WeaponStatComposer.Compose(CreateBaseStats(), modifiers);

            Assert.AreEqual(0f, stats.Damage, Tolerance);
            Assert.AreEqual(0f, stats.HipSpreadAngle, Tolerance);
        }

        [Test]
        public void Compose_MagazineCapacityBelowOne_IsAtLeastOne()
        {
            var modifiers = new[] { Add(WeaponStat.MagazineCapacity, -100f) };

            var stats = WeaponStatComposer.Compose(CreateBaseStats(), modifiers);

            Assert.AreEqual(0f, stats.Get(WeaponStat.MagazineCapacity), Tolerance);
            Assert.AreEqual(1, stats.MagazineCapacity);
        }

        [Test]
        public void Compose_FractionalMagazineCapacity_IsRounded()
        {
            var modifiers = new[] { Multiply(WeaponStat.MagazineCapacity, 0.25f) };

            var stats = WeaponStatComposer.Compose(CreateBaseStats(), modifiers);

            // 30 * 1.25 = 37.5
            Assert.AreEqual(38, stats.MagazineCapacity);
        }

        [Test]
        public void Compose_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() =>
                WeaponStatComposer.Compose(null, Array.Empty<WeaponStatModifier>()));
            Assert.Throws<ArgumentNullException>(() => WeaponStatComposer.Compose(CreateBaseStats(), null));
        }

        private static void AssertEqual(WeaponStats expected, WeaponStats actual)
        {
            Assert.AreEqual(expected.Damage, actual.Damage);
            Assert.AreEqual(expected.MagazineCapacity, actual.MagazineCapacity);
            Assert.AreEqual(expected.HipSpreadAngle, actual.HipSpreadAngle);
            Assert.AreEqual(expected.AimSpreadAngle, actual.AimSpreadAngle);
        }
    }
}
