﻿using System;

namespace SSCMS
{
    public class Token
    {
        public string Type { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }
    }
}