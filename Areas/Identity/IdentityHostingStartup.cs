using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutorBuddy.Areas.Identity.Data;
using TutorBuddy.Models;

[assembly: HostingStartup(typeof(TutorBuddy.Areas.Identity.IdentityHostingStartup))]
namespace TutorBuddy.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
           /* builder.ConfigureServices((context, services) => {
              });*/
        }
    }
}