namespace FoodOrdering.Domain.Enums
{
    public enum UserRole
    {
        Customer = 1,
        Admin = 2,
        Chef = 3,
        Waiter = 4,
        DeliveryPerson = 5
    }

    public enum TableLocationType
    {
        Indoor = 1,
        Outdoor = 2,
        VIPBooth = 3,
        Terrace = 4
    }

    public enum TableStatus
    {
        Available = 1,
        Reserved = 2,
        Occupied = 3,
        Maintenance = 4
    }

    public enum FoodSizeOption
    {
        Standard = 1,
        HalfPortion = 2,
        FullPortion = 3,
        ChefTastingSet = 4
    }

    public enum SaleType
    {
        DineIn = 1,
        Takeaway = 2,
        Delivery = 3
    }

    public enum SaleStatus
    {
        Placed = 1,
        PaymentPending = 2,
        Paid = 3,
        Preparing = 4,
        Ready = 5,
        Shipped = 6,
        Delivered = 7,
        Completed = 8,
        Cancelled = 9
    }

    public enum ReservationStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3,
        Seated = 4,
        Completed = 5
    }

    public enum NotificationType
    {
        System = 1,
        OrderUpdate = 2,
        ReservationUpdate = 3,
        Promotion = 4
    }

    public enum DiscountType
    {
        Percentage = 1,
        FixedAmount = 2
    }
}
