﻿using System;

namespace Messenger.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Guid AvatarId { get; set; }
    }
}
