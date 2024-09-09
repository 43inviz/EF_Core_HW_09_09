using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection.Metadata.Ecma335;

namespace Try1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DbManager db = new DbManager();

            //db.EnsurePopulate();

            var guest = db.GetGuest(1);

            var events = db.GetEvents(1);


            //Add Guest on event

            db.AddGuestOnEvent(guest,events);


            //Get guests by event
            var guests = db.GetGuestByEvent(1);


            //Change Guest role

            db.ChangeGuestRole(1, 1, GuestRole.VIP);

            //Get events by guest

            var eventsByGuest = db.GetEventByGuest(1);

            //Remove guest from event

            db.RemoveGuestFromEvent(1, 1);

            //Get events by guest role

            var eventsByRole = db.GetEventByRole(1, GuestRole.Guest);
            
            

        }



    }

    public class Events
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Guest> Guests { get; set; } = new List<Guest>();

        public List<EventsGuests> EventsGuests { get; set; } = new();
        
    }


    public class Guest
    {
        public int Id { get; set; }
        public string Name { set; get; }

        public List<Events> Events { get; set; } = new();

        public List<EventsGuests> EventsGuests { get; set; } = new();
    }



    public class EventsGuests
    {
        public int GuestsId { get; set; }

        public Guest Guests { get; set; }
        public int EventId { get; set; }

        public Events Events { get; set; }

        public GuestRole Role { get; set; }

    }


    public enum GuestRole
    {
        Guest,
        VIP
    }


    public class ApplicationContext : DbContext
    {
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Events> Events { get; set; }

        public DbSet<EventsGuests> EventsGuests { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-R3LQDV9;Database = TestDb1;Trusted_Connection =True;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Events>().HasMany(e => e.Guests).WithMany(g => g.Events).UsingEntity<EventsGuests>
                (
                g => g.HasOne(e => e.Guests).WithMany(e => e.EventsGuests).HasForeignKey(e => e.GuestsId),
                e => e.HasOne(e => e.Events).WithMany(e => e.EventsGuests).HasForeignKey(e => e.EventId),
                eg =>
                {
                    eg.Property(e => e.Role).HasDefaultValue(GuestRole.Guest);
                    eg.HasKey(k => new { k.EventId, k.GuestsId });
                    eg.ToTable("EventsGuest");
                }

                );
        }
                
    }


    public class DbManager
    {
        public void EnsurePopulate()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var events = new List<Events>
                {
                    new Events {Name = "Event1"},
                    new Events {Name = "Event2"}

                };

                var guest = new List<Guest>
                {
                    new Guest {Name = "Tom" },
                    new Guest {Name = "Alex "},
                    new Guest {Name = "Oleg"}

                };

                var eventsGuest = new List<EventsGuests>
                {
                    new EventsGuests { Events = events[0], Guests = guest[0], Role = GuestRole.Guest },
                    new EventsGuests {Events = events[0],Guests = guest[1],Role = GuestRole.VIP},
                    new EventsGuests{Events = events[1],Guests = guest[1],Role = GuestRole.Guest},
                    new EventsGuests {Events = events[1],Guests = guest[2],Role = GuestRole.VIP},
                };


                db.Events.AddRange(events);

                db.Guests.AddRange(guest);

                db.EventsGuests.AddRange(eventsGuest);
                db.SaveChanges();
            }
        }

        public Guest? GetGuest(int id)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return db.Guests.FirstOrDefault(g => g.Id == id);
            }
        }


        public Events? GetEvents(int id) 
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return db.Events.FirstOrDefault(e => e.Id == id);
            }
        }


        public void AddGuestOnEvent(Guest guest,Events events)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                events.Guests.Add(guest);
                db.SaveChanges();
            }
        }


        public Events? GetGuestByEvent(int eventId)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                
                return db.Events.Include(e => e.Guests).FirstOrDefault(e => e.Id == eventId);
            }
        }


        public Guest? GetEventByGuest(int guestId)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return db.Guests.Include(e => e.Events).FirstOrDefault(e=>e.Id == guestId);
            }
        }


        public void ChangeGuestRole(int guestId,int eventId,GuestRole role)
        {
            using(ApplicationContext db = new ApplicationContext())
            {
                var eventGuest = db.EventsGuests.FirstOrDefault(e => e.EventId == eventId && e.GuestsId == guestId);
                if(eventGuest!= null)
                {
                    eventGuest.Role = role;
                    db.SaveChanges();
                }
            }
        }


        public void RemoveGuestFromEvent(int guestId, int eventId)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var eventGuest = db.EventsGuests.FirstOrDefault(e => e.EventId == 1 && e.GuestsId == guestId);
                if (eventGuest != null)
                {
                    db.EventsGuests.Remove(eventGuest);
                    db.SaveChanges();
                }
            }
        }


        public List<Events> GetEventByRole(int guestId,GuestRole role)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                return db.EventsGuests.Where(e=>e.GuestsId == guestId && e.Role == role)
                    .Select(e=>e.Events).ToList();

            }
        }



    }
        


}
