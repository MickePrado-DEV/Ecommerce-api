namespace Ecommerce.Application.Authorization
{
    public static class AdminPermissions
    {
        public const string DashboardView = "admin.dashboard.view";
        public const string CoversView = "admin.covers.view";
        public const string CoversManage = "admin.covers.manage";
        public const string FamiliesView = "admin.families.view";
        public const string FamiliesManage = "admin.families.manage";
        public const string CategoriesView = "admin.categories.view";
        public const string CategoriesManage = "admin.categories.manage";
        public const string SubcategoriesView = "admin.subcategories.view";
        public const string SubcategoriesManage = "admin.subcategories.manage";
        public const string ProductsView = "admin.products.view";
        public const string ProductsManage = "admin.products.manage";
        public const string OptionsView = "admin.options.view";
        public const string OptionsManage = "admin.options.manage";
        public const string StockView = "admin.stock.view";
        public const string StockManage = "admin.stock.manage";
        public const string DriversView = "admin.drivers.view";
        public const string DriversManage = "admin.drivers.manage";
        public const string OrdersView = "admin.orders.view";
        public const string OrdersManage = "admin.orders.manage";
        public const string ShipmentsView = "admin.shipments.view";
        public const string ShipmentsManage = "admin.shipments.manage";

        public static readonly string[] All =
        [ DashboardView, CoversView, CoversManage, FamiliesView, FamiliesManage, CategoriesView, CategoriesManage, SubcategoriesView, SubcategoriesManage,
                 ProductsView, ProductsManage, OptionsView, OptionsManage, StockView, StockManage, DriversView, DriversManage, OrdersView, OrdersManage, ShipmentsView, ShipmentsManage
               ];
    }
}
