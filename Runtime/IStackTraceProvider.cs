﻿namespace GenericScriptableArchitecture
{
    using System.Collections.Generic;

    internal interface IStackTraceProvider
    {
        bool Enabled { get; set; }

        bool Expanded { get; set; }

        ICollection<StackTraceEntry> Entries { get; }
    }
}