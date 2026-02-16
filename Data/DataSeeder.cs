using DesertPaths.Constants;
using DesertPaths.Models.Entities;
using DesertPaths.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace DesertPaths.Data;

public static class DataSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in AppRoles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var adminEmail = configuration["AdminUser:Email"] ?? "admin@desertpaths.com";
        var adminPassword = configuration["AdminUser:Password"] ?? "Admin123!";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
            }
        }
    }

    public static async Task SeedJourneyStylesAsync(ApplicationDbContext context)
    {
        if (!context.JourneyStyles.Any())
        {
            var styles = new List<JourneyStyle>
            {
                new()
                {
                    Name = "Comfort",
                    Description = "Travel in comfort with premium accommodations and services",
                    PriceMultiplier = 1.0m
                },
                new()
                {
                    Name = "Premium",
                    Description = "Enhanced experience with luxury touches and exclusive access",
                    PriceMultiplier = 1.5m
                },
                new()
                {
                    Name = "Luxury",
                    Description = "Ultimate luxury experience with the finest accommodations",
                    PriceMultiplier = 2.0m
                }
            };

            context.JourneyStyles.AddRange(styles);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedLandsAndJourneysAsync(ApplicationDbContext context)
    {
        if (!context.Lands.Any())
        {
            var comfortStyle = context.JourneyStyles.First(s => s.Name == "Comfort");

            var lands = new List<Land>
            {
                new()
                {
                    Name = "Saudi Arabia",
                    Slug = "saudi-arabia",
                    ShortDescription = "Discover the vast Empty Quarter and ancient Nabatean ruins of AlUla.",
                    FullDescription = "<p>Saudi Arabia offers some of the most dramatic desert landscapes on Earth. From the towering red sandstone formations of AlUla to the endless dunes of the Rub' al Khali (Empty Quarter), this ancient land has captivated travelers for millennia.</p><p>Experience the warm hospitality of Bedouin culture, explore UNESCO World Heritage sites, and witness starlit nights unlike anywhere else on Earth.</p>",
                    HeroImageUrl = "https://images.unsplash.com/photo-1509316785289-025f5b846b35?w=1920&h=1080&fit=crop",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1509316785289-025f5b846b35?w=600&h=400&fit=crop",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new()
                {
                    Name = "Jordan",
                    Slug = "jordan",
                    ShortDescription = "Walk through the rose-red city of Petra and camp under Wadi Rum stars.",
                    FullDescription = "<p>Jordan is a land of ancient wonders and timeless beauty. The rose-red city of Petra, carved into towering sandstone cliffs, stands as one of the New Seven Wonders of the World.</p><p>Beyond Petra, the desert wilderness of Wadi Rum offers dramatic landscapes that have served as the backdrop for countless films. Sleep under the stars in Bedouin camps and experience the magic of this extraordinary land.</p>",
                    HeroImageUrl = "https://images.unsplash.com/photo-1547234935-80c7145ec969?w=1920&h=1080&fit=crop",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1547234935-80c7145ec969?w=600&h=400&fit=crop",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new()
                {
                    Name = "Morocco",
                    Slug = "morocco",
                    ShortDescription = "Experience the magic of Saharan dunes and vibrant Berber culture.",
                    FullDescription = "<p>Morocco's Sahara Desert offers an unforgettable journey into one of Earth's most iconic landscapes. The golden dunes of Erg Chebbi and Erg Chigaga rise majestically against brilliant blue skies.</p><p>Travel through ancient kasbahs, experience traditional Berber hospitality, and ride camels into the sunset. Morocco's desert is a place of profound beauty and timeless traditions.</p>",
                    HeroImageUrl = "https://images.unsplash.com/photo-1548018560-c7196c4cbc30?w=1920&h=1080&fit=crop",
                    ThumbnailUrl = "https://images.unsplash.com/photo-1548018560-c7196c4cbc30?w=600&h=400&fit=crop",
                    DisplayOrder = 3,
                    IsActive = true
                }
            };

            context.Lands.AddRange(lands);
            await context.SaveChangesAsync();

            // Add Journeys
            var saudiArabia = context.Lands.First(l => l.Slug == "saudi-arabia");
            var jordan = context.Lands.First(l => l.Slug == "jordan");
            var morocco = context.Lands.First(l => l.Slug == "morocco");

            var journeys = new List<Journey>
            {
                new()
                {
                    Title = "AlUla Discovery",
                    Slug = "alula-discovery",
                    LandId = saudiArabia.Id,
                    DefaultStyleId = comfortStyle.Id,
                    ShortDescription = "Explore the ancient Nabatean tombs and stunning rock formations of AlUla.",
                    FullDescription = "<p>Journey through time in AlUla, home to the remarkable Hegra (Madain Salih), Saudi Arabia's first UNESCO World Heritage Site. Marvel at intricately carved Nabatean tombs dating back 2,000 years.</p><p>This journey combines archaeological wonders with luxury desert camps and unforgettable stargazing experiences.</p>",
                    DurationDays = 5,
                    DurationNights = 4,
                    PriceFrom = 2500,
                    MaxGroupSize = 12,
                    Difficulty = DifficultyLevel.Easy,
                    HeroImageUrl = "https://images.unsplash.com/photo-1509316785289-025f5b846b35?w=1920&h=1080&fit=crop",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    Title = "Empty Quarter Expedition",
                    Slug = "empty-quarter-expedition",
                    LandId = saudiArabia.Id,
                    DefaultStyleId = comfortStyle.Id,
                    ShortDescription = "Venture into the world's largest sand desert for an epic adventure.",
                    FullDescription = "<p>The Rub' al Khali, or Empty Quarter, is the largest contiguous sand desert in the world. This expedition takes you deep into this magnificent wilderness.</p><p>Experience the thrill of dune driving, traditional Bedouin camps, and landscapes that seem to stretch to infinity.</p>",
                    DurationDays = 7,
                    DurationNights = 6,
                    PriceFrom = 3500,
                    MaxGroupSize = 8,
                    Difficulty = DifficultyLevel.Moderate,
                    HeroImageUrl = "https://images.unsplash.com/photo-1509316785289-025f5b846b35?w=1920&h=1080&fit=crop",
                    IsFeatured = false,
                    IsActive = true
                },
                new()
                {
                    Title = "Petra & Wadi Rum",
                    Slug = "petra-wadi-rum",
                    LandId = jordan.Id,
                    DefaultStyleId = comfortStyle.Id,
                    ShortDescription = "Discover the wonders of Petra and the dramatic landscapes of Wadi Rum.",
                    FullDescription = "<p>Begin your journey at Petra, the ancient Nabatean city carved into rose-red cliffs. Walk through the Siq to witness the Treasury in all its glory.</p><p>Continue to Wadi Rum, where towering sandstone mountains rise from the desert floor. Sleep in luxury Bedouin camps beneath star-filled skies.</p>",
                    DurationDays = 6,
                    DurationNights = 5,
                    PriceFrom = 2800,
                    MaxGroupSize = 14,
                    Difficulty = DifficultyLevel.Moderate,
                    HeroImageUrl = "https://images.unsplash.com/photo-1547234935-80c7145ec969?w=1920&h=1080&fit=crop",
                    IsFeatured = true,
                    IsActive = true
                },
                new()
                {
                    Title = "Sahara Dreams",
                    Slug = "sahara-dreams",
                    LandId = morocco.Id,
                    DefaultStyleId = comfortStyle.Id,
                    ShortDescription = "Journey through Morocco's stunning Sahara Desert and ancient kasbahs.",
                    FullDescription = "<p>Cross the High Atlas Mountains and descend into the Sahara Desert. Visit the magnificent kasbahs of Ait Benhaddou and explore the palm-lined Draa Valley.</p><p>End your journey with camel rides into the golden dunes of Erg Chebbi, where you'll spend the night in a traditional desert camp.</p>",
                    DurationDays = 5,
                    DurationNights = 4,
                    PriceFrom = 1800,
                    MaxGroupSize = 16,
                    Difficulty = DifficultyLevel.Easy,
                    HeroImageUrl = "https://images.unsplash.com/photo-1548018560-c7196c4cbc30?w=1920&h=1080&fit=crop",
                    IsFeatured = true,
                    IsActive = true
                }
            };

            context.Journeys.AddRange(journeys);
            await context.SaveChangesAsync();

            // Add sample itineraries for first journey
            var alulaJourney = context.Journeys.First(j => j.Slug == "alula-discovery");
            var itineraries = new List<JourneyItinerary>
            {
                new()
                {
                    JourneyId = alulaJourney.Id,
                    DayNumber = 1,
                    Title = "Arrival in AlUla",
                    Description = "Arrive at AlUla airport and transfer to your luxury desert resort. Afternoon at leisure to relax and acclimate. Evening welcome dinner featuring traditional Arabian cuisine.",
                    Highlights = "Desert resort check-in, Welcome dinner"
                },
                new()
                {
                    JourneyId = alulaJourney.Id,
                    DayNumber = 2,
                    Title = "Hegra UNESCO Site",
                    Description = "Full day exploration of Hegra (Madain Salih), featuring over 100 well-preserved Nabatean tombs. Learn about the ancient civilization that once thrived here.",
                    Highlights = "Qasr al-Farid tomb, Ancient inscriptions, Nabatean history"
                },
                new()
                {
                    JourneyId = alulaJourney.Id,
                    DayNumber = 3,
                    Title = "Old Town & Elephant Rock",
                    Description = "Morning visit to AlUla Old Town with its abandoned mudbrick houses. Afternoon jeep safari to Elephant Rock and surrounding formations. Sunset dinner in the desert.",
                    Highlights = "Old Town walk, Elephant Rock, Desert dinner"
                },
                new()
                {
                    JourneyId = alulaJourney.Id,
                    DayNumber = 4,
                    Title = "Desert Adventures",
                    Description = "Choose your adventure: hiking, mountain biking, or a hot air balloon ride over the desert. Evening stargazing session with an astronomy guide.",
                    Highlights = "Adventure activities, Stargazing experience"
                },
                new()
                {
                    JourneyId = alulaJourney.Id,
                    DayNumber = 5,
                    Title = "Departure",
                    Description = "Leisurely breakfast and time for last-minute exploration or shopping. Transfer to AlUla airport for your departure.",
                    Highlights = "Local souvenirs, Departure"
                }
            };

            context.JourneyItineraries.AddRange(itineraries);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedAllAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, configuration);
        await SeedJourneyStylesAsync(context);
        await SeedLandsAndJourneysAsync(context);
    }
}
