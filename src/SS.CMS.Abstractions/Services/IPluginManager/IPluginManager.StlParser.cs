﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SS.CMS.Services
{
    public partial interface IPluginManager
    {
        Task<Dictionary<string, Func<IParseContext, string>>> GetParsesAsync();
    }
}
