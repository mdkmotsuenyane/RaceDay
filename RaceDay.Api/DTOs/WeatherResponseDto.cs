namespace RaceDay.DTOs
{
    public class WeatherResponseDto
    {
        public int EventID { get; set; }
        public string? WeatherForecast { get; set; }
        public decimal? Temperature { get; set; }
        public int? WindSpeed { get; set; }
        public int? Precipitation { get; set; }
    }
}