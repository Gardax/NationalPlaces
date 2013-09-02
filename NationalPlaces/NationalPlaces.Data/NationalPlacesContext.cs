using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalPlaces.Data.Migrations;
using NationalPlaces.Models;

namespace NationalPlaces.Data
{
    public class NationalPlacesContext:DbContext
    {
        public NationalPlacesContext()
            :base("NationalPlacesDb")
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Town> Towns { get; set; }
        public DbSet<Picture> Pictures { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<NationalPlacesContext, Configuration>());
            base.OnModelCreating(modelBuilder);
        }
    }
}
