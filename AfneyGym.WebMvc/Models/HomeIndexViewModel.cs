using AfneyGym.Common.DTOs;

namespace AfneyGym.WebMvc.Models;

public class HomeIndexViewModel
{
    public string HeroVariant { get; set; } = "a";
    public LandingKpiDto Kpis { get; set; } = new();
}

