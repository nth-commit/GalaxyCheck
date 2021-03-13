﻿using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Test ForThese(Func<bool> func) => new TestImpl(
            (_) => func(),
            null!,
            0);

        public static Test ForThese(Action func) => ForThese(func.AsTrueFunc());

        public static Property<object> FromLinq(IGen<Test> testGen) =>
            new Property(testGen, new PropertyOptions { EnableLinqInference = true });
    }
}