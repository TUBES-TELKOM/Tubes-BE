using Microsoft.Extensions.Configuration;
using Tubes_POS_API.Options;

namespace Tubes_POS_API.Tests;

public class ApiOptionsTests
{
    // Tests that ApiOptions binds correctly from configuration.
    [Fact]
    public void ApiOptions_ShouldBindFromConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Api:Name"] = "Custom API",
                ["Api:Version"] = "v2",
                ["Api:Description"] = "Custom description"
            })
            .Build();

        var options = config.GetSection("Api").Get<ApiOptions>();

        Assert.NotNull(options);
        Assert.Equal("Custom API", options!.Name);
        Assert.Equal("v2", options.Version);
        Assert.Equal("Custom description", options.Description);
    }
}
