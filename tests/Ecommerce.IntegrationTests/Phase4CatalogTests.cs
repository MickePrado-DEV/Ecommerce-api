using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.IntegrationTests;

public class Phase4CatalogTests(EcommerceWebApplicationFactory factory) : IClassFixture<EcommerceWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Product_Detail_Includes_Options_And_ResolveVariant()
    {
        var detail = await _client.GetFromJsonAsync<JsonElement>(
            "/api/v1/catalog/products/audifonos-pro-x", JsonOptions);
        Assert.True(detail.GetProperty("options").GetArrayLength() >= 1);
        var optionValueId = detail.GetProperty("options")[0].GetProperty("values")[0].GetProperty("id").GetString()!;

        var resolve = await _client.PostAsJsonAsync(
            "/api/v1/catalog/products/audifonos-pro-x/resolve-variant",
            new { optionValueIds = new[] { Guid.Parse(optionValueId) } });
        resolve.EnsureSuccessStatusCode();
        var variant = await resolve.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.False(string.IsNullOrEmpty(variant.GetProperty("variantId").GetString()));
    }

    [Fact]
    public async Task Wishlist_Review_And_Coupon_Checkout()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "cliente@ecommerce.local", password = "Cliente123!" });
        var loginBody = await login.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = loginBody.GetProperty("accessToken").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var detail = await _client.GetFromJsonAsync<JsonElement>(
            "/api/v1/catalog/products/audifonos-pro-x", JsonOptions);
        var productId = detail.GetProperty("id").GetString()!;
        var variantId = detail.GetProperty("variants")[0].GetProperty("id").GetString()!;

        var addWish = await _client.PostAsync($"/api/v1/wishlist/{productId}", null);
        addWish.EnsureSuccessStatusCode();
        var wishlist = await _client.GetFromJsonAsync<JsonElement>("/api/v1/wishlist", JsonOptions);
        Assert.True(wishlist.GetArrayLength() >= 1);

        var reviewBeforeDelivery = await _client.PostAsJsonAsync("/api/v1/catalog/products/audifonos-pro-x/reviews",
            new { rating = 5, title = "Excelente", comment = "Muy buenos audífonos" });
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, reviewBeforeDelivery.StatusCode);

        await _client.PostAsJsonAsync("/api/v1/cart/items",
            new { variantId = Guid.Parse(variantId), quantity = 1 });

        var checkout = await _client.PostAsJsonAsync("/api/v1/checkout", new
        {
            fullName = "Cliente Demo",
            street = "Calle 1",
            city = "CDMX",
            state = "CDMX",
            postalCode = "01000",
            country = "MX",
            phone = "5551234567",
            shippingCost = 10m,
            couponCode = "WELCOME10"
        });
        checkout.EnsureSuccessStatusCode();
        var checkoutBody = await checkout.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("WELCOME10", checkoutBody.GetProperty("couponCode").GetString());
        Assert.True(checkoutBody.GetProperty("discountAmount").GetDecimal() > 0);
    }
}
