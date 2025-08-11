using System;
namespace ShippingBook.Core.Entities
{
    public class MaerskRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Commodity { get; set; }
        public string ContainerType { get; set; }
        public string ContainerCount { get; set; }
        public string CargoWeight { get; set; }
        public bool IsPriceOwner { get; set; }
        public DateTime CargoDate { get; set; }
    }
}

