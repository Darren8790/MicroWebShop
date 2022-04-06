namespace WebMVC.Services;

public static class API
{
    public static class Catalogue
    {
        public static string GetAllCatalogueItems(string baseUri, int page, int take) //int page, int take
        {
            return $"{baseUri}items?pageIndex={page}&pageSize={take}";
        }
    }

    public static class Basket
    {
        public static string GetBasket(string baseUri, string basketId) => $"{baseUri}/{basketId}";
        //public static string GetBasket(string baseUri) => $"{baseUri}/";
        public static string UpdateBasket(string baseUri) => baseUri;
        public static string CheckoutBasket(string baseUri) => $"{baseUri}/checkout";
        public static string CleanBasket(string baseUri, string basketId) => $"{baseUri}/{basketId}";
    }

    public static class Purchase
    {
        public static string AddItemToBasket(string baseUri) => $"{baseUri}/basket/items";
        public static string UpdateBasketItem(string baseUri) => $"{baseUri}/basket/items";
        public static string GetOrderDraft(string baseUri, string basketId) => $"{baseUri}/order/draft/{basketId}";
    }

    public static class Order
    {
        public static string GetOrder(string baseUri, string orderId) => $"{baseUri}/{orderId}";
        public static string GetAllOrders(string baseUri) => $"{baseUri}";
        public static string AddNewOrder(string baseUri) => $"{baseUri}/new";
        public static string CancelOrder(string baseUri) => $"{baseUri}/cancel";
        public static string ShipOrder(string baseUri) => $"{baseUri}/ship";
    }
}