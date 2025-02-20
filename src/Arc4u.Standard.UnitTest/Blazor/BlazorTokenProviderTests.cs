using System.IdentityModel.Tokens.Jwt;
using Arc4u.Blazor;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.TokenProvider;
using AutoFixture;
using AutoFixture.AutoMoq;
using Blazored.LocalStorage;
using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace Arc4u.UnitTest.Blazor;

[Trait("Category", "CI")]
public class BlazorTokenProviderTests
{
    public BlazorTokenProviderTests()
    {
        fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture fixture;

    [Fact]
    public void JwtSecurityTokenShould()
    {
        // arrange
        var jwt = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        // act
        var jwt2 = new JwtSecurityToken(accessToken);

        // assert
        jwt2.EncodedPayload.Should().BeEquivalentTo(jwt.EncodedPayload);
    }

    [Fact]
    public async Task GetValidTokenShoud()
    {
        // Arrange
        var jwt = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var tokenInfo = new TokenInfo("Bearer", accessToken, DateTime.UtcNow);

        Dictionary<string, string> keySettings = [];
        keySettings.Add(TokenKeys.AuthorityKey, "http://sts");
        keySettings.Add(TokenKeys.RedirectUrl, "https://localhost:44444/");

        var mockLocalStorage = fixture.Freeze<Mock<ILocalStorageService>>();
        mockLocalStorage.Setup(p => p.GetItemAsStringAsync("token", It.IsAny<CancellationToken>())).Returns(ValueTask.FromResult<string?>(accessToken));
        mockLocalStorage.Setup(p => p.RemoveItemAsync("token", It.IsAny<CancellationToken>()));

        var mockInterop = fixture.Freeze<Mock<ITokenWindowInterop>>();
        mockInterop.Setup(m => m.OpenWindowAsync(It.IsAny<IJSRuntime>(), It.IsAny<ILocalStorageService>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

        var mockKeyValueSettings = fixture.Freeze<Mock<IKeyValueSettings>>();
        mockKeyValueSettings.SetupGet(p => p.Values).Returns(keySettings);

        var sut = fixture.Create<BlazorTokenProvider>();

        // act
        var token = await sut.GetTokenAsync(mockKeyValueSettings.Object, null);

        // assert
        token.Should().NotBeNull();
        token!.Token.Should().Be(accessToken);

        mockInterop.Verify(m => m.OpenWindowAsync(It.IsAny<IJSRuntime>(), It.IsAny<ILocalStorageService>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        mockLocalStorage.Verify(p => p.GetItemAsStringAsync("token", It.IsAny<CancellationToken>()), Times.Once);
        mockLocalStorage.Verify(p => p.RemoveItemAsync("token", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ObsoleteAccessTokenInTheCacheShoud()
    {
        // Arrange
        var jwtExpired = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));
        var expiredAccessToken = new JwtSecurityTokenHandler().WriteToken(jwtExpired);

        var jwt = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        Dictionary<string, string> keySettings = [];
        keySettings.Add(TokenKeys.AuthorityKey, "http://sts");
        keySettings.Add(TokenKeys.RedirectUrl, "https://localhost:44444/");

        var mockLocalStorage = fixture.Freeze<Mock<ILocalStorageService>>();
        mockLocalStorage.SetupSequence(p => p.GetItemAsStringAsync("token", It.IsAny<CancellationToken>()))
                        .Returns(ValueTask.FromResult<string?>(expiredAccessToken))
                        .Returns(ValueTask.FromResult<string?>(accessToken));
        mockLocalStorage.Setup(p => p.RemoveItemAsync("token", It.IsAny<CancellationToken>()));

        var mockInterop = fixture.Freeze<Mock<ITokenWindowInterop>>();
        mockInterop.Setup(m => m.OpenWindowAsync(It.IsAny<IJSRuntime>(), It.IsAny<ILocalStorageService>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

        var mockKeyValueSettings = fixture.Freeze<Mock<IKeyValueSettings>>();
        mockKeyValueSettings.SetupGet(p => p.Values).Returns(keySettings);

        var sut = fixture.Create<BlazorTokenProvider>();

        // act
        var token = await sut.GetTokenAsync(mockKeyValueSettings.Object, null);

        // assert
        token.Should().NotBeNull();
        token!.Token.Should().Be(accessToken);

        mockInterop.Verify(m => m.OpenWindowAsync(It.IsAny<IJSRuntime>(), It.IsAny<ILocalStorageService>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mockLocalStorage.Verify(p => p.GetItemAsStringAsync("token", It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockLocalStorage.Verify(p => p.RemoveItemAsync("token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NoTokenInTheCacheShoud()
    {
        // Arrange
        var jwt = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var tokenInfo = new TokenInfo("Bearer", accessToken, DateTime.UtcNow);

        Dictionary<string, string> keySettings = [];
        keySettings.Add(TokenKeys.AuthorityKey, "http://sts");
        keySettings.Add(TokenKeys.RedirectUrl, "https://localhost:44444/");

        var mockLocalStorage = fixture.Freeze<Mock<ILocalStorageService>>();
        mockLocalStorage.SetupSequence(p => p.GetItemAsStringAsync("token", It.IsAny<CancellationToken>()))
                        .Returns(ValueTask.FromResult<string?>(default!))
                        .Returns(ValueTask.FromResult<string?>(accessToken));
        mockLocalStorage.Setup(p => p.RemoveItemAsync("token", It.IsAny<CancellationToken>()));

        var mockInterop = fixture.Freeze<Mock<ITokenWindowInterop>>();
        mockInterop.Setup(m => m.OpenWindowAsync(It.IsAny<IJSRuntime>(), It.IsAny<ILocalStorageService>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

        var mockKeyValueSettings = fixture.Freeze<Mock<IKeyValueSettings>>();
        mockKeyValueSettings.SetupGet(p => p.Values).Returns(keySettings);

        var sut = fixture.Create<BlazorTokenProvider>();

        // act
        var token = await sut.GetTokenAsync(mockKeyValueSettings.Object, null);

        // assert
        token.Should().NotBeNull();
        token!.Token.Should().Be(accessToken);

        mockInterop.Verify(m => m.OpenWindowAsync(It.IsAny<IJSRuntime>(), It.IsAny<ILocalStorageService>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mockLocalStorage.Verify(p => p.GetItemAsStringAsync("token", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task NoValidAccessTokenInTheCacheShoud()
    {
        // Arrange
        var jwt = new JwtSecurityToken("issuer", "audience", [new("key", "value")], notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddHours(1));

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var tokenInfo = new TokenInfo("Bearer", accessToken, DateTime.UtcNow);

        Dictionary<string, string> keySettings = [];
        keySettings.Add(TokenKeys.AuthorityKey, "http://sts");
        keySettings.Add(TokenKeys.RedirectUrl, "https://localhost:44444/");

        var mockLocalStorage = fixture.Freeze<Mock<ILocalStorageService>>();
        mockLocalStorage.SetupSequence(p => p.GetItemAsStringAsync("token", It.IsAny<CancellationToken>()))
                        .Returns(ValueTask.FromResult<string?>(jwt.EncodedPayload)) // wrong access token
                        .Returns(ValueTask.FromResult<string?>(accessToken));

        mockLocalStorage.Setup(p => p.RemoveItemAsync("token", It.IsAny<CancellationToken>()));

        var mockInterop = fixture.Freeze<Mock<ITokenWindowInterop>>();
        mockInterop.Setup(m => m.OpenWindowAsync(It.IsAny<IJSRuntime>(), It.IsAny<ILocalStorageService>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

        var mockKeyValueSettings = fixture.Freeze<Mock<IKeyValueSettings>>();
        mockKeyValueSettings.SetupGet(p => p.Values).Returns(keySettings);

        var sut = fixture.Create<BlazorTokenProvider>();

        // act
        var token = await sut.GetTokenAsync(mockKeyValueSettings.Object, null);

        // assert
        token.Should().NotBeNull();
        token!.Token.Should().Be(accessToken);

        mockInterop.Verify(m => m.OpenWindowAsync(It.IsAny<IJSRuntime>(), It.IsAny<ILocalStorageService>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mockLocalStorage.Verify(p => p.GetItemAsStringAsync("token", It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockLocalStorage.Verify(p => p.RemoveItemAsync("token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTokenWithNullSettingsValuesShoud()
    {
        // Arrange
        Dictionary<string, string> keySettings = [];

        var mockKeyValueSettings = fixture.Freeze<Mock<IKeyValueSettings>>();
        mockKeyValueSettings.SetupGet(p => p.Values).Returns(keySettings);

        var sut = fixture.Create<BlazorTokenProvider>();

        // act
        var exception = await Record.ExceptionAsync(async () => await sut.GetTokenAsync(mockKeyValueSettings.Object, null));

        // assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task GetTokenWithNullSettingsShoud()
    {
        // Arrange
        var sut = fixture.Create<BlazorTokenProvider>();

        // act
        var exception = await Record.ExceptionAsync(async () => await sut.GetTokenAsync(null, null));

        // assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentNullException>();
    }
}
