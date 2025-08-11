using System;
namespace ShippingBook.Core.Entities
{
    public class MaerskResponse
    {
        public List<FreightCharge> Charges { get; set; } = new List<FreightCharge>();

        public class FreightCharge
        {
            public string ChargeType { get; set; }  // Örneğin: "Basic Ocean Freight", "Terminal Handling Service - Origin"
            public string Basis { get; set; }       // Örneğin: "Container", "Bill of Lading"
            public int Quantity { get; set; }       // Örneğin: 2, 1
            public string Currency { get; set; }    // Örneğin: "USD", "EUR", "XOF"
            public decimal UnitPrice { get; set; }  // Örneğin: 2559, 400, 15
            public decimal TotalPrice { get; set; } // Örneğin: 5118, 800, 30
        }
    }
}

