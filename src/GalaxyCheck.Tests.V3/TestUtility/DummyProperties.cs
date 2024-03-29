﻿namespace GalaxyCheck.Tests.TestUtility;

public static class DummyProperties
{
    public static Property<int> EventuallyFalsified(Spy<int>? spy = null) => Gen.Int32().Between(0, 100).ForAll(x =>
    {
        spy?.NotifyInvoked(x);
        return x < 50;
    });
}
