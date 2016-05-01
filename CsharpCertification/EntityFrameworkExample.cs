using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter3
{
    public class EntityFrameworkExample
    {
        public void Run()
        {
            using (var ctx = new GameCtx())
            {
                var wpn1 = new Weapon
                {
                    Damage = 10,
                    Range = 20,
                };

                var wpn2 = new Weapon
                {
                    Damage = 2,
                    Range = 40,
                };

                var chr = new Character
                {
                    Name = "John Doe",
                    Health = 9001,
                    MainWeapon = wpn1, // commenting will give validation error
                    SecondaryWeapon = wpn2
                };

                ctx.Characters.Add(chr);
                ctx.SaveChanges();

                var c = (from ch in ctx.Characters
                         where ch.MainWeapon != null
                         select new
                         {
                             Name = ch.Name,
                             Health = ch.Health,
                             Id = ch.Id
                         }).FirstOrDefault();

                Console.WriteLine(string.Format("Found character '{0}' with over '{1}' health left. Id '{2}'.", c.Name, c.Health - 1, c.Id));
            }
        }
    }

    class GameCtx : DbContext
    {
        public IDbSet<Character> Characters { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Character>().HasRequired(c => c.MainWeapon).WithMany().WillCascadeOnDelete(false);
        }
    }

    class Character
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Name { get; set; }

        [Required]
        public int Health { get; set; }

        [Required]
        public Weapon MainWeapon { get; set; }

        public Weapon SecondaryWeapon { get; set; }

    }

    class Weapon
    {
        public int Id { get; set; }

        [Required]
        public int Damage { get; set; }

        [Required]
        public int Range { get; set; }
    }
}
