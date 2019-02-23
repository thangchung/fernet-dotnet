# Fernet Token for .NET Core

Authenticated and encrypted API token using modern crypto.

[![Software License](https://img.shields.io/badge/license-MIT-brightgreen.svg?style=flat-square)](LICENSE.md)

## What?

[Fernet](https://github.com/fernet/spec) which takes a user-provided message (an arbitrary sequence of bytes), a key (256 bits), and the current time, and produces a token, which contains the message in a form that can't be read or altered without the key.

## Build

Build the library using .NET SDK.

```bash
$ cd src\Fernet
$ dotnet restore
```

## Testing

You can run tests either manually or automatically on every code change.

```bash
$ cd src\FernetTests
$ dotnet test
```

## Usage

- Plaintext message

```csharp
var key = SimpleFernet.GenerateKey().UrlSafe64Decode();
var src = "hello";
var src64 = src.ToBase64String();

var token = SimpleFernet.Encrypt(key, src64.UrlSafe64Decode());
var decoded64 = SimpleFernet.Decrypt(key, token, out var timestamp);
var decoded = decoded64.UrlSafe64Encode().FromBase64String();
```

- Array message

```csharp
var key = SimpleFernet.GenerateKey().UrlSafe64Decode();
var src = "[ 'id': '123456' ]";
var src64 = src.ToBase64String();

var token = SimpleFernet.Encrypt(key, src64.UrlSafe64Decode());
var decoded64 = SimpleFernet.Decrypt(key, token, out var timestamp);
var decoded = decoded64.UrlSafe64Encode().FromBase64String();
```

### Integrate with IdentityServer 4 (ID4)

ID4 is an OpenID Connect and OAuth 2.0 Framework for ASP.NET Core. At the moment, it's not supporting Fernet token provider, then the solution for it is wrapping the JWT token inside the fernet token (I'm not sure it is a good solution, but it is a temperary solution working now). See the sample project for it in `samples` folder

- [x] Custom `DefaultTokenCreationService` class to do the fernet encryption.

```csharp
public class MyTokenCreationService : DefaultTokenCreationService
{
    public MyTokenCreationService(ISystemClock clock, IKeyMaterialService keys, ILogger<DefaultTokenCreationService> logger)
        : base(clock, keys, logger)
    {
    }

    protected override async Task<string> CreateJwtAsync(JwtSecurityToken jwt)
    {
        var jwtToken = await base.CreateJwtAsync(jwt);
        var jwt64Token = jwtToken.ToBase64String();

        // this key should store in the KeyVault service, then we can securely access in anywhere
        var key = "cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4=".UrlSafe64Decode();
        var fernetToken = SimpleFernet.Encrypt(key, jwt64Token.UrlSafe64Decode());

        return fernetToken;
    }
}
```

In `Startup.cs`

```csharp
services.AddSingleton<ITokenCreationService, MyTokenCreationService>();
```

- [x] Write a middleware in `SampleApi` to catch the token before ID4 can get it, and decrypt it to normally JWT token.

```csharp
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].ToString();

    if (!string.IsNullOrEmpty(token))
    {
        var fernetToken = token.Substring("bearer".Length + 1, token.Length - "bearer".Length - 1);

        // this key should store in the KeyVault service, then we can securely access in anywhere
        var key = "cw_0x689RpI-jtRR7oE8h_eQsKImvJapLeSbXpwF4e4=".UrlSafe64Decode();
        var jwt64Token = SimpleFernet.Decrypt(key, fernetToken, out var timestamp);
        var jwtToken = jwt64Token.UrlSafe64Encode().FromBase64String();

        // we set it to authorization header, then the internal stack will work normally
        context.Request.Headers["Authorization"] = $"Bearer {jwtToken}";
    }

    await next.Invoke();
});
```

Run 3 projects: `IdentityServer4`, `SampleApi`, and `ConsoleApp`, you will see as below

![id4_fernet](artwork/id4_fernet.PNG?raw=true 'id4_fernet')

Look into the `ConsoleApp`, you should see that we can access to `SampleApi` data. Happy hacking!

_Notes_: we're still working on it, so please be care of using it on the production mode. That would be great if you can contact with us to discuss a best solution.

## Contributing

Please see [CONTRIBUTING](CONTRIBUTING.md) for details.

## Security

If you discover any security related issues, please email thangchung.onthenet@gmail.com instead of using the issue tracker.

## License

The MIT License (MIT). Please see [License File](LICENSE.md) for more information.
