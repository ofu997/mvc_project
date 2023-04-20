using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Exp.Models;

namespace Exp.Data
{
    public class ExpContext : DbContext
    {
        public ExpContext (DbContextOptions<ExpContext> options)
            : base(options)
        {
        }

        public DbSet<Exp.Models.Response> Response { get; set; } = default!;
    }
}
