# <img src="logo/ogu-logo.png" alt="Header" width="24"/> Ogu.Otp

[![.NET](https://github.com/ogulcanturan/Ogu.Otp/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/ogulcanturan/Ogu.Otp/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/Ogu.Otp.svg?color=1ecf18)](https://nuget.org/packages/Ogu.Otp)
[![Nuget](https://img.shields.io/nuget/dt/Ogu.Otp.svg?logo=nuget)](https://nuget.org/packages/Ogu.Otp)

## Introduction

This library offers support for both Time-Based One-Time Password (`TOTP`) and HMAC-based one-time password (`HOTP`) protocols, enabling secure and reliable authentication for various applications. With seamless integration for `Google`, `Microsoft`, and other `RFC 6238-compliant` authentication systems


## Features

- TOTP (MD5, SHA1, SHA256, SHA384, SHA512)
- HOTP (MD5, SHA1, SHA256, SHA384, SHA512)
- Includes Base32Helper class

## Installation

You can install the library via NuGet Package Manager:

```bash
dotnet add package Ogu.Otp
```
## Usage

Totp:

```csharp
private static readonly TotpTokenProvider _totp = new TotpTokenProvider("MyAppName");

var secretKey = _totp.GenerateSecretKey(); // Save this for further evaluations

string uri = _totp.GetUri("Ogulcan", secretKey); // Needed for generating qr code

// (optional) manually you can generate code ( e.g time based email verification ) but this generally returns from Auth app (Google, Microsoft)
var code = _totp.GenerateCode(secretKey); 

var otpValidationResult = _totp.ValidateCode(code, secretKey); // validates the code and returns the validation result
```

Hotp: 

```csharp
private static readonly HotpTokenProvider _hotp = new HotpTokenProvider("MyAppName");

var secretKey = _hotp.GenerateSecretKey(); // Save this for further evaluations

string uri = _hotp.GetUri("Ogulcan", secretKey); // Needed for generating qr code

// (optional) manually you can generate code ( e.g time based email verification ) but this generally returns from Auth app (Google, Microsoft)
var code = _hotp.GenerateCode(secretKey);

long counter = 0;

var otpValidationResult = _hotp.ValidateCode(code, secretKey, counter);

if (otpValidationResult.IsValid)
{
    counter++; // increase the counter and save it to database
}
```

## Sample Application
A sample application demonstrating the usage of Ogu.Otp [Console](https://github.com/ogulcanturan/Ogu.Otp/blob/master/samples/Otp.Console/), [Otp.Api](https://github.com/ogulcanturan/Ogu.Otp/blob/master/samples/Otp.Api/).

## Credits

This library makes use of certain codes and resources from [AspNetCore](https://github.com/dotnet/aspnetcore), [Base32.cs](https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/Base32.cs), [Rfc6238AuthenticationService.cs](https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/Rfc6238AuthenticationService.cs)
