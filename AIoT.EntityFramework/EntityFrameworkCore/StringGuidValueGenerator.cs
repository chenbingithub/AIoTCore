using System;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    public class StringGuidValueGenerator : ValueGenerator<string>
    {
        public override bool GeneratesTemporaryValues => false;

        public override string Next(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) => Guid.NewGuid().ToString("N");
    }
}
