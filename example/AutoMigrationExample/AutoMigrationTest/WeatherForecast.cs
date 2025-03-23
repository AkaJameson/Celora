using System.ComponentModel.DataAnnotations;

namespace AutoMigrationTest
{
    public class WeatherForecast
    {
        [Key]
        public int Id { get; set; }
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }

        public string Reverse1 { get; set; }
    }
}
