using System;
using AutoFixture;

namespace PactTest.CommandLine.Tests
{
    internal class DeterministicFixture : Fixture
    {
        public DeterministicFixture(int seed)
        {
            var random = new Random(seed);

            this.Register(() => random.Next());
            this.Register(() => $"string_{random.Next()}");
        }
    }
}