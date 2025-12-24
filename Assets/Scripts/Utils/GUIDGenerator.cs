using System;

public static class GUIDGenerator
{
    public static string NewGUID() => Guid.NewGuid().ToString();
}
