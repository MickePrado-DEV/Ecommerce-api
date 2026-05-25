using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.IntegrationTests;

public class CriticalFlowTests(EcommerceWebApplicationFactory factory) : IClassFixture<EcommerceWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_Admin_ReturnsTokenWithPermissions()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "admin@ecommerce.local", password = "Admin123!" });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(json.GetProperty("accessToken").GetString()?.Length > 0);
        Assert.True(json.GetProperty("permissions").GetArrayLength() > 0);
    }

    [Fact]
    public async Task Admin_Dashboard_WithoutPermission_Returns403()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "cliente@ecommerce.local", password = "Cliente123!" });
        var body = await login.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = body.GetProperty("accessToken").GetString();

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/dashboard");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(req);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_Customer_AssignsCustomerRole()
    {
        var email = $"cliente-{Guid.NewGuid():N}@test.local";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register/customer", new
        {
            email,
            password = "Cliente123!",
            firstName = "Nuevo",
            lastName = "Cliente",
            phone = "+5215559999"
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var roles = json.GetProperty("user").GetProperty("roles");
        Assert.Contains(roles.EnumerateArray(), r => r.GetString() == "customer");
    }

    [Fact]
    public async Task Login_Driver_ReturnsDriverId()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "repartidor@ecommerce.local",
            password = "Repartidor123!"
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(json.GetProperty("user").GetProperty("driverId").GetGuid() != Guid.Empty);
    }

    [Fact]
    public async Task Update_Profile_And_Cancel_Order()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "cliente@ecommerce.local", password = "Cliente123!" });
        var loginBody = await login.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = loginBody.GetProperty("accessToken").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var patch = await _client.PatchAsJsonAsync("/api/v1/auth/me", new
        {
            firstName = "Cliente",
            lastName = "Actualizado",
            phone = "+5215551111"
        });
        patch.EnsureSuccessStatusCode();
        var me = await patch.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.Equal("Actualizado", me.GetProperty("lastName").GetString());

        var products = await _client.GetFromJsonAsync<JsonElement>("/api/v1/catalog/products?page=1&pageSize=1", JsonOptions);
        var slug = products.GetProperty("items")[0].GetProperty("slug").GetString()!;
        var detail = await _client.GetFromJsonAsync<JsonElement>($"/api/v1/catalog/products/{slug}", JsonOptions);
        var variantId = detail.GetProperty("variants")[0].GetProperty("id").GetString()!;

        await _client.PostAsJsonAsync("/api/v1/cart/items", new { variantId = Guid.Parse(variantId), quantity = 1 });
        var checkout = await _client.PostAsJsonAsync("/api/v1/checkout", new
        {
            fullName = "Cliente Demo",
            street = "Calle 1",
            city = "CDMX",
            state = "CDMX",
            postalCode = "01000",
            country = "MX",
            phone = "5551234567",
            shippingCost = 10m
        });
        checkout.EnsureSuccessStatusCode();
        var orderId = (await checkout.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("orderId").GetString()!;

        var cancel = await _client.PostAsync($"/api/v1/orders/{orderId}/cancel", null);
        cancel.EnsureSuccessStatusCode();

        var list = await _client.GetFromJsonAsync<JsonElement>($"/api/v1/orders?status=Cancelled&page=1&pageSize=10", JsonOptions);
        Assert.True(list.GetProperty("total").GetInt32() >= 1);
    }

    [Fact]
    public async Task Catalog_And_Cart_Checkout_Pay_Flow()
    {
        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "cliente@ecommerce.local", password = "Cliente123!" });
        var loginBody = await login.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = loginBody.GetProperty("accessToken").GetString()!;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var products = await _client.GetFromJsonAsync<JsonElement>("/api/v1/catalog/products?page=1&pageSize=10", JsonOptions);
        var slug = products.GetProperty("items")[0].GetProperty("slug").GetString()!;

        var detail = await _client.GetFromJsonAsync<JsonElement>($"/api/v1/catalog/products/{slug}", JsonOptions);
        var variantId = detail.GetProperty("variants")[0].GetProperty("id").GetString()!;

        var cartAdd = await _client.PostAsJsonAsync("/api/v1/cart/items", new { variantId = Guid.Parse(variantId), quantity = 1 });
        cartAdd.EnsureSuccessStatusCode();

        var checkout = await _client.PostAsJsonAsync("/api/v1/checkout", new
        {
            fullName = "Cliente Demo",
            street = "Calle 1",
            city = "CDMX",
            state = "CDMX",
            postalCode = "01000",
            country = "MX",
            phone = "5551234567",
            shippingCost = 10m
        });
        checkout.EnsureSuccessStatusCode();
        var checkoutBody = await checkout.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var orderId = checkoutBody.GetProperty("orderId").GetString()!;

        var pay = await _client.PostAsync($"/api/v1/orders/{orderId}/pay", null);
        pay.EnsureSuccessStatusCode();
    }
}
