using ARENDATOR.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace ARENDATOR.Web.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "Owner");

        var adminEmail = "admin@arendator.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                PhoneNumber = "+15550000001",
                FirstName = "System",
                LastName = "Admin",
                IsAdmin = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var ownerEmail = "owner@arendator.com";
        var owner = await userManager.FindByEmailAsync(ownerEmail);
        if (owner == null)
        {
            owner = new ApplicationUser
            {
                UserName = ownerEmail,
                Email = ownerEmail,
                PhoneNumber = "+15550000002",
                FirstName = "Rental",
                LastName = "Owner",
                IsOwner = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(owner, "Owner123!");
            await userManager.AddToRoleAsync(owner, "Owner");
        }

        if (dbContext.Properties.Count() < 40)
        {
            var propertyList = BuildSampleCatalog(owner.Id);
            dbContext.Properties.AddRange(propertyList);
            await dbContext.SaveChangesAsync();
        }
    }

    private static List<Property> BuildSampleCatalog(string ownerId)
    {
        var properties = new List<Property>
        {
            CreateApartment("Skyline Residence", 3, 2, 120, 180m, "New York", "250 Fifth Avenue, Manhattan, NY", PropertyStatus.Available, "https://images.unsplash.com/photo-1494526585095-c41746248156?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=1200&q=80", true, true, "Penthouse"),
            CreateApartment("Downtown Penthouse", 2, 2, 95, 95m, "Chicago", "45 Michigan Avenue, Chicago, IL", PropertyStatus.Rented, "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=80", true, false, "Penthouse"),
            CreateApartment("Riverside Apartment", 2, 1, 85, 80m, "Seattle", "12 Pike Street, Seattle, WA", PropertyStatus.Available, "https://images.unsplash.com/photo-1460317442991-0ec209397118?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80", true, false, "Apartment"),
            CreateApartment("Central Luxury Flat", 3, 2, 110, 130m, "Boston", "88 Tremont Street, Boston, MA", PropertyStatus.Available, "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80", true, true, "Flat"),
            CreateApartment("Cozy Studio", 1, 1, 40, 35m, "Austin", "15 Congress Avenue, Austin, TX", PropertyStatus.Available, "https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494526585095-c41746248156?auto=format&fit=crop&w=1200&q=80", false, false, "Studio"),
            CreateApartment("Mountain View Apartment", 2, 2, 90, 75m, "Denver", "75 Walnut Street, Denver, CO", PropertyStatus.Maintenance, "https://images.unsplash.com/photo-1472224371017-08207f84aaae?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=1200&q=80", true, false, "Apartment"),
            CreateApartment("Family Comfort Home", 3, 2, 125, 100m, "San Diego", "44 Harbor View Road, San Diego, CA", PropertyStatus.Available, "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=1200&q=80", true, true, "Home"),
            CreateApartment("Modern City Suite", 1, 1, 55, 50m, "Miami", "300 Ocean Drive, Miami, FL", PropertyStatus.Rented, "https://images.unsplash.com/photo-1494526585095-c41746248156?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=80", true, false, "Suite"),
            CreateApartment("Garden View Residence", 2, 2, 80, 70m, "Portland", "55 Rose Street, Portland, OR", PropertyStatus.Available, "https://images.unsplash.com/photo-1448630360428-65456885c650?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80", true, true, "Residence"),
            CreateApartment("Executive Penthouse", 4, 3, 160, 220m, "Los Angeles", "900 Sunset Boulevard, Los Angeles, CA", PropertyStatus.Available, "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1460317442991-0ec209397118?auto=format&fit=crop&w=1200&q=80", true, true, "Penthouse"),
            CreateApartment("Sunrise Apartment", 2, 2, 75, 65m, "Phoenix", "18 Palm Avenue, Phoenix, AZ", PropertyStatus.Rented, "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494526585095-c41746248156?auto=format&fit=crop&w=1200&q=80", true, false, "Apartment"),
            CreateApartment("Royal Residence", 3, 3, 135, 170m, "Las Vegas", "210 Bellagio Lane, Las Vegas, NV", PropertyStatus.Available, "https://images.unsplash.com/photo-1460317442991-0ec209397118?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80", true, true, "Residence"),
            CreateApartment("Minimalist Studio", 1, 1, 38, 30m, "Seattle", "210 Pine Street, Seattle, WA", PropertyStatus.Available, "https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80", false, false, "Studio"),
            CreateApartment("Business Class Flat", 2, 2, 88, 90m, "New York", "120 Park Avenue, New York, NY", PropertyStatus.Rented, "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80", true, false, "Flat"),
            CreateApartment("Lakeview Residence", 3, 2, 115, 140m, "Minneapolis", "65 Lakeview Drive, Minneapolis, MN", PropertyStatus.Available, "https://images.unsplash.com/photo-1448630360428-65456885c650?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494526585095-c41746248156?auto=format&fit=crop&w=1200&q=80", true, true, "Residence"),
            CreateApartment("Smart Living Apartment", 2, 2, 78, 85m, "San Francisco", "30 Market Street, San Francisco, CA", PropertyStatus.Maintenance, "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=80", true, false, "Apartment"),
            CreateApartment("Premium Family Suite", 3, 2, 130, 150m, "Dallas", "70 Cedar Avenue, Dallas, TX", PropertyStatus.Available, "https://images.unsplash.com/photo-1460317442991-0ec209397118?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494526585095-c41746248156?auto=format&fit=crop&w=1200&q=80", true, true, "Suite"),
            CreateApartment("Old Town Apartment", 2, 1, 70, 60m, "Philadelphia", "18 Walnut Street, Philadelphia, PA", PropertyStatus.Rented, "https://images.unsplash.com/photo-1472224371017-08207f84aaae?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=80", true, false, "Apartment"),
            CreateApartment("Grand Luxury Residence", 4, 3, 180, 250m, "Miami", "900 Oceanfront Avenue, Miami, FL", PropertyStatus.Available, "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1460317442991-0ec209397118?auto=format&fit=crop&w=1200&q=80", true, true, "Residence"),
            CreateApartment("Urban Comfort Studio", 1, 1, 45, 40m, "Chicago", "70 South Wacker, Chicago, IL", PropertyStatus.Available, "https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494526585095-c41746248156?auto=format&fit=crop&w=1200&q=80", false, false, "Studio"),
            CreateCar("BMW 5 Series", "BMW", "5 Series", 2024, "Automatic", "Hybrid", 115m, "Miami", "220 Biscayne Blvd, Miami, FL", PropertyStatus.Available, "https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Mercedes-Benz E-Class", "Mercedes-Benz", "E-Class", 2023, "Automatic", "Petrol", 130m, "Berlin", "14 Breite Straße, Berlin", PropertyStatus.Available, "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Toyota Camry", "Toyota", "Camry", 2024, "Automatic", "Hybrid", 65m, "Los Angeles", "430 Wilshire Blvd, Los Angeles, CA", PropertyStatus.Rented, "https://images.unsplash.com/photo-1544636331-e26879cd4d9b?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494905998402-395d579af36f?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Range Rover Sport", "Land Rover", "Range Rover Sport", 2023, "Automatic", "Diesel", 150m, "Dubai", "1 Marina Crescent, Dubai", PropertyStatus.Available, "https://images.unsplash.com/photo-1511919884226-fd3cad34687c?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Hyundai Sonata", "Hyundai", "Sonata", 2023, "Automatic", "Petrol", 55m, "Austin", "9 South Congress, Austin, TX", PropertyStatus.Rented, "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494905998402-395d579af36f?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Toyota Land Cruiser", "Toyota", "Land Cruiser", 2022, "Automatic", "Diesel", 180m, "Doha", "12 Corniche Street, Doha", PropertyStatus.Available, "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1489824904134-891ab64532f1?auto=format&fit=crop&w=1200&q=80", 7),
            CreateCar("Kia K5", "Kia", "K5", 2024, "Automatic", "Petrol", 60m, "Seoul", "8 Gangnam Avenue, Seoul", PropertyStatus.Available, "https://images.unsplash.com/photo-1494905998402-395d579af36f?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Mercedes-Benz S-Class", "Mercedes-Benz", "S-Class", 2023, "Automatic", "Hybrid", 250m, "Paris", "15 Avenue des Champs-Élysées, Paris", PropertyStatus.Available, "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Chevrolet Malibu", "Chevrolet", "Malibu", 2022, "Automatic", "Petrol", 45m, "Chicago", "18 Loop Plaza, Chicago, IL", PropertyStatus.Maintenance, "https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Lexus RX 350", "Lexus", "RX 350", 2024, "Automatic", "Hybrid", 140m, "Toronto", "20 Bay Street, Toronto", PropertyStatus.Available, "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Honda Accord", "Honda", "Accord", 2023, "Automatic", "Petrol", 55m, "San Diego", "270 Harbor Drive, San Diego, CA", PropertyStatus.Rented, "https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494905998402-395d579af36f?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("BMW X5", "BMW", "X5", 2023, "Automatic", "Diesel", 160m, "London", "44 Regent Street, London", PropertyStatus.Available, "https://images.unsplash.com/photo-1511919884226-fd3cad34687c?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Toyota Corolla", "Toyota", "Corolla", 2024, "Automatic", "Hybrid", 35m, "New York", "9 Madison Avenue, New York, NY", PropertyStatus.Available, "https://images.unsplash.com/photo-1494905998402-395d579af36f?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Audi A6", "Audi", "A6", 2023, "Automatic", "Petrol", 120m, "Munich", "8 Königsplatz, Munich", PropertyStatus.Rented, "https://images.unsplash.com/photo-1544636331-e26879cd4d9b?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Porsche Cayenne", "Porsche", "Cayenne", 2022, "Automatic", "Diesel", 200m, "Dubai", "88 Sheikh Zayed Road, Dubai", PropertyStatus.Available, "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1511919884226-fd3cad34687c?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Hyundai Elantra", "Hyundai", "Elantra", 2024, "Automatic", "Hybrid", 40m, "Austin", "100 West 6th Street, Austin, TX", PropertyStatus.Available, "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494905998402-395d579af36f?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Lexus ES 350", "Lexus", "ES 350", 2023, "Automatic", "Hybrid", 110m, "Tokyo", "21 Shibuya, Tokyo", PropertyStatus.Rented, "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1553440569-bcc63803a83d?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Toyota RAV4", "Toyota", "RAV4", 2024, "Automatic", "Hybrid", 70m, "Vancouver", "210 Robson Street, Vancouver", PropertyStatus.Available, "https://images.unsplash.com/photo-1511919884226-fd3cad34687c?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1494905998402-395d579af36f?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Mercedes-Benz G-Class", "Mercedes-Benz", "G-Class", 2022, "Automatic", "Diesel", 280m, "Munich", "80 Leopoldstraße, Munich", PropertyStatus.Available, "https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1503376780353-7e6692767b70?auto=format&fit=crop&w=1200&q=80", 5),
            CreateCar("Kia Sportage", "Kia", "Sportage", 2024, "Automatic", "Hybrid", 55m, "Boston", "40 Beacon Street, Boston, MA", PropertyStatus.Maintenance, "https://images.unsplash.com/photo-1544636331-e26879cd4d9b?auto=format&fit=crop&w=1200&q=80", "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?auto=format&fit=crop&w=1200&q=80", 5),
        };

        for (var index = 0; index < properties.Count; index++)
        {
            properties[index].OwnerId = ownerId;
            properties[index].CreatedAt = DateTime.UtcNow.AddDays(-(index + 1));
            properties[index].Featured = index < 6;
            properties[index].Country = "USA";
            properties[index].ContactName = "Owner Services";
            properties[index].ContactPhone = "+1 (555) 010-0000";
            properties[index].ContactEmail = "owner@arendator.com";
        }

        return properties;
    }

    private static Property CreateApartment(
        string title,
        int bedrooms,
        int bathrooms,
        int areaSqMeters,
        decimal pricePerDay,
        string city,
        string address,
        PropertyStatus status,
        string coverImageUrl,
        string secondImageUrl,
        bool hasParking,
        bool hasPool,
        string apartmentType)
    {
        return new Property
        {
            Title = title,
            Slug = title.Replace(" ", "-").ToLowerInvariant(),
            Description = $"{title} combines modern comfort with a prime location and flexible rental terms for short or extended stays.",
            Category = PropertyCategory.Apartment,
            Status = status,
            PricePerDay = pricePerDay,
            Currency = "USD",
            City = city,
            Address = address,
            Country = "USA",
            CoverImageUrl = coverImageUrl,
            Featured = true,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            MaxGuests = bedrooms + 1,
            AreaSqMeters = areaSqMeters,
            ApartmentType = apartmentType,
            HasParking = hasParking,
            HasPool = hasPool,
            Images = new List<PropertyImage>
            {
                new PropertyImage { ImageUrl = coverImageUrl, IsPrimary = true, Caption = "Main photo" },
                new PropertyImage { ImageUrl = secondImageUrl, IsPrimary = false, Caption = "Additional photo" }
            }
        };
    }

    private static Property CreateCar(
        string title,
        string brand,
        string model,
        int year,
        string transmission,
        string fuelType,
        decimal pricePerDay,
        string city,
        string address,
        PropertyStatus status,
        string coverImageUrl,
        string secondImageUrl,
        int seats)
    {
        return new Property
        {
            Title = title,
            Slug = title.Replace(" ", "-").ToLowerInvariant(),
            Description = $"{title} offers premium comfort, reliable performance, and elegant design for daily business or leisure driving.",
            Category = PropertyCategory.Car,
            Status = status,
            PricePerDay = pricePerDay,
            Currency = "USD",
            City = city,
            Address = address,
            Country = "USA",
            CoverImageUrl = coverImageUrl,
            Featured = true,
            Brand = brand,
            Model = model,
            Year = year,
            Transmission = transmission,
            FuelType = fuelType,
            Mileage = 5000 + (year - 2022) * 3500,
            BodyType = "Sedan",
            Seats = seats,
            Images = new List<PropertyImage>
            {
                new PropertyImage { ImageUrl = coverImageUrl, IsPrimary = true, Caption = "Main photo" },
                new PropertyImage { ImageUrl = secondImageUrl, IsPrimary = false, Caption = "Exterior photo" }
            }
        };
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
