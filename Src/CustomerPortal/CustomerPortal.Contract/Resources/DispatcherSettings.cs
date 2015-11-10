using System.ComponentModel.DataAnnotations;

namespace CustomerPortal.Contract.Resources
{
    public class DispatcherSettings
    {
        public DispatcherSettings()
        {
            NumberOfOffersPerCycle = 0;
            NumberOfCycles = 1;
            DurationOfOfferInSeconds = 15;
        }

        [Required]
        [Display(Name = "Number of Offers Per Cycle (N)", Description = "A value of 0 means we are not handling the dispatch ourselves")]
        [Range(0, 10)]
        public int NumberOfOffersPerCycle { get; set; }

        [Required]
        [Display(Name = "Number of Cycles (C)")]
        [Range(1, 100)]
        public int NumberOfCycles { get; set; }

        [Required]
        [Display(Name = "Duration of Offer In Seconds (D)")]
        [Range(10, 60)]
        public int DurationOfOfferInSeconds { get; set; }
    }
}