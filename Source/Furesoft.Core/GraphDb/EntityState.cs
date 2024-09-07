﻿namespace Furesoft.Core.GraphDb;

[Flags]
public enum EntityState : byte
{
    Unchanged = 0,
    Added = 1,
    Modified = 2,
    Deleted = 4
}