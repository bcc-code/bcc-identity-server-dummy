// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Bcc.Members.Identity.Domain.Quickstart.Users;
using IdentityModel;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Quickstart.UI
{
    public class TestUsers
    {
        private readonly UserManager<AppUser> userManager;
        public TestUsers(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task AddRandomUsersToTheUserStore()
        {
            for (int i = 1; i < 6000; i++)
            {
                var user = new AppUser()
                {
                    Id = i.ToString("D5"),
                    Email = $"testuser-{i.ToString("D5")}-@gmail.com",
                    Password = "password",
                    UserName = $"testuser-{i.ToString("D5")}-@gmail.com",
                    churchId = "69",
                    churchName = "Oslo/Follo",
                    DateOfBirth = "1988-12-08T00:00:00",
                    EmailConfirmed = true,
                    hasMembership = true,
                    Name = "Test User",
                    FirstName = "Test",
                    LastName = "User",
                    personId = i.ToString("D5")                    
                };
                var identityResult = await userManager.CreateAsync(user);

                Debug.Write($"Generated {user.personId} user \n");
            };
            
        }
    }
}