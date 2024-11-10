namespace MediFinder_Backend.ModelosEspeciales
{
    public class AddSale
    {
        public class AddSaleDTO
        {
            public int ProductId { get; set; }

            public int Quantity { get; set; }

            public decimal UnitPrice { get; set; }

            public decimal discountPercentage { get; set; }
        }

    }
}
