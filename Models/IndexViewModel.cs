using AriaPrint.Repository;

namespace AriaPrint.Models
{
    public class IndexViewModel
    {
        public List<News> News { get; set; }
        public List<ServicesCard> Services { get; set; }
        public List<WhyUsCard> WhyUs { get; set; }
    }
}
