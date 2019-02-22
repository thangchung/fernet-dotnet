# Fernet Token for .NET Core

Authenticated and encrypted API token using modern crypto.

[![Software License](https://img.shields.io/badge/license-MIT-brightgreen.svg?style=flat-square)](LICENSE.md)

## What?

[Fernet](https://github.com/fernet/spec) which takes a user-provided message (an arbitrary sequence of bytes), a key (256 bits), and the current time, and produces a token, which contains the message in a form that can't be read or altered without the key.

## Install

Install the library using .NET SDK.

```bash
$ dotnet restore
$ dotnet run
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

## Testing

You can run tests either manually or automatically on every code change.

```bash
$ dotnet test
```

## Contributing

Please see [CONTRIBUTING](CONTRIBUTING.md) for details.

## Security

If you discover any security related issues, please email thangchung.onthenet@gmail.com instead of using the issue tracker.

## License

The MIT License (MIT). Please see [License File](LICENSE.md) for more information.
