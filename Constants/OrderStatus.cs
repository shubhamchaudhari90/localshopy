namespace localshopyNew.Constants
{
    public class OrderStatus
    {
        //Order Placed -> Processing -> Shipped -> Out for Delivery -> Delivered

        public const string ORDER_PLACED = "Order Placed";

        public const string ACCEPTED = "Accepted";
        public const string CANCELLED = "Cancelled";
        public const string REJECTED = "Rejected";

        public const string PROCESSING = "Processing";
        public const string SHIPPED = "Shipped";
        public const string OUT_FOR_DELIVERY = "Out for Delivery";
        public const string DELIVERED = "Delivered";
    }
}
