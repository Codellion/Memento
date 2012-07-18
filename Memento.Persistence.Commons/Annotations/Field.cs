﻿using System;
using System.Data;

namespace Memento.Persistence.Commons.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Field : Attribute
    {
        public string Name;
        public bool Required;
    }
}