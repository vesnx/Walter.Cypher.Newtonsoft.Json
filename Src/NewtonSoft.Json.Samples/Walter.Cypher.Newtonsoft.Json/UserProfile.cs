// ***********************************************************************
// Assembly         : ProfileApp
// Author           : Walter Verhoeven
// Created          : Fri 01-Mar-2024
//
// Last Modified By : Walter Verhoeven
// Last Modified On : Fri 01-Mar-2024
// ***********************************************************************
// <copyright file="UserProfile.cs" company="VESNX SA">
//     Copyright (c) VESNX SA. All rights reserved.
// </copyright>
// <summary>
// show how to configure a class using annotation to obfuscate the properties
// as well as it's content to be compliant with GDPR and privacy laws
// </summary>
// ***********************************************************************
using Newtonsoft.Json;

using System.Net;
namespace ProfileApp;

internal record UserProfile
{
    [JsonProperty("a")]
    [JsonConverter(typeof(GDPRObfuscatedStringConverter))]
    public required string Name { get; set; }

    [JsonProperty("b")]
    [JsonConverter(typeof(GDPRObfuscatedStringConverter))]
    public required string Email { get; set; }

    [JsonProperty("c")]
    [JsonConverter(typeof(GDPRObfuscatedDateTimeConverter))]
    public DateTime DateOfBirth { get; set; }
    
    [JsonProperty("d")]
    [JsonConverter(typeof(GDPRIPAddressListConverter))]
    public List<IPAddress> Devices { get; set; } = [];
}

