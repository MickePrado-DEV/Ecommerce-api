foreach (var pwd in new[] { "Admin123!", "Cliente123!", "Repartidor123!" })
    Console.WriteLine($"{pwd} => {BCrypt.Net.BCrypt.HashPassword(pwd)}");
