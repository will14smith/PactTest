namespace PactTest.Web.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Person { get; set; }
        public string Item { get; set; }
        public bool Delivered { get; set; }
    }
}