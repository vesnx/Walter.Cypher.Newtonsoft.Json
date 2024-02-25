
# Walter.Cypher.Newtonsoft.Json

The `Walter.Cypher.Newtonsoft.Json` NuGet package provides a set of custom converters designed to enhance the security and privacy of your .NET applications by protecting sensitive data, especially useful in adhering to GDPR requirements. These converters can be easily integrated with the `Newtonsoft.Json` serialization and deserialization process, obfuscating sensitive information such as IP addresses, strings, dates, and numbers.


## Available Converters

- **GDPRCollectionOfStringConverter**: Protects GDPR sensitive string collections, ideal for personal data like email lists.
- **GDPRIPAddressConverter**: Obfuscates IP addresses to ensure privacy and compliance.
- **GDPRIPAddressListConverter**: Safeguards lists of IP addresses, useful for configuration data or logging.
- **GDPRObfuscatedDateTimeConverter**: Obfuscates dates, suitable for sensitive date information like birthdates or issue dates.
- **GDPRObfuscatedStringConverter**: General purpose obfuscation for single strings, applicable to names, credit card numbers, etc.
- **GDPRObfuscatedIntConverter**: Protects sensitive integer values, such as credit card CCVs.


## Getting Started
To use this package, first install it via NuGet
```c#
Install-Package Walter.Cypher.Newtonsoft.Json
```

## Use Case: Secure User Profile Serialization
This example demonstrates configuring System.Newtonsoft.Json to use the provided GDPR converters for both serialization and deserialization processes, ensuring that sensitive data is adequately protected according to GDPR guidelines.


You can then use these converters in a class or record, the bellow sample uses a combination of property names and converters to remove any inferable information from the Json string
```c#
    using Newtonsoft.Json;

    [JsonObject( MemberSerialization.OptIn)]
    record ProfileData
    {

        [JsonConstructor]
        internal ProfileData(List<IPAddress> a, string b, IPAddress c, int d)
        {
            AddressRange = a;
            SecretText = b;
            SingleAddress = c;
            CCV = d;

            //use Walter NuGet package to inverse inject ILogger
            //if no logger injected a default target will be used
            //depending the OS different targets are used, in windows
            //this is the Application event log
            Inverse.GetLogger("ProfileData")?.LogInformation("Json constructor called");
        }


        [JsonProperty("a")]
        [JsonConverter(typeof(GDPRIPAddressListConverter))]
        public List<IPAddress> AddressRange { get; }

        [JsonProperty("b")]
        [JsonConverter(typeof(GDPRObfuscatedStringConverter))]
        public string SecretText { get; }

        [JsonProperty("c")]
        [JsonConverter(typeof(GDPRIPAddressConverter))]
        public IPAddress SingleAddress { get; }

        [JsonProperty("d")]
        [JsonConverter(typeof(GDPRObfuscatedIntConverter))]
        public int CCV { get; }


    }
```

Optionally you can use dependency injection to integrate the Walter framework and use a shared secret password for the json encryption

 ```c#
    //secure the json using a protected password
    using var service = new ServiceCollection()
                            .AddLogging(option =>
                            {
                                //configure your logging as you would normally do
                                option.AddConsole();
                                option.SetMinimumLevel(LogLevel.Information);
                            })
                            //set the application password for json encryption
                            .UseSymmetricEncryption("May$ecr!tPa$$w0rd")                                    
                            .BuildServiceProvider();

    service.AddLoggingForWalter();//enable logging for the Walter framework classes
```

## Serializing and Deserializing Securely
No specific configuration is need as we have defined the converter attributes on the properties.
 ```c#
         //set the options you need. in this sample we us a internal constructor
         JsonSerializerSettings _options = new JsonSerializerSettings()
         {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
         };

            /*
            save to json and store or send to a insecure location the profile to disk. 
            note that data in transit can be read even if TLS secured using proxy or man in the middle. 
             */
            var profile = new UserProfile() { 
                Email = "My@email.com", 
                Name = "Jo Coder", 
                DateOfBirth = new DateTime(2001, 7, 16), 
                Devices=[IPAddress.Parse("192.168.1.1"),IPAddress.Parse("192.168.1.14"), IPAddress.Loopback]
            };

            var json= JsonConvert.SerializeObject(profile, _options);
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MySupperApp");
            var fileName = Path.Combine(directory, "Data1.json");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            //use inversion of control and generate a ILogger without dependency injection
            Inverse.GetLogger("MyConsoleApp")?.LogInformation("Cyphered json:\n{json}", json);


            await File.WriteAllTextAsync(fileName, json).ConfigureAwait(false);
```

## Reading and Validating Data
Read the encrypted JSON from storage or after transmission, and deserialize it back into the UserProfile class, automatically decrypting and validating the data.
 ```c#
var cypheredJson = await File.ReadAllTextAsync("path_to_encrypted_json").ConfigureAwait(false);

//use the string extension from the Walter NuGet package to try and serialize the object
if (cypheredJson.IsValidJson<UserProfile>(_options,out var user))
{
    // Use the deserialized and decrypted `user` object
}
```

## A working copy for use in a console application

The following is a working example of how you could use this in a console application
```c# 

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


 //secure the json using a protected password
using var service = new ServiceCollection()
                        .AddLogging(option =>
                        {
                            //configure your logging as you would normally do
                            option.AddConsole();
                            option.SetMinimumLevel(LogLevel.Information);
                        })
                        .UseSymmetricEncryption("May$ecr!tPa$$w0rd")
                        .BuildServiceProvider();

service.AddLoggingForWalter();//enable logging for the Walter framework classes


JsonSerializerSettings options = new JsonSerializerSettings()
{
    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
};



//... rest of your code 

/*
save to json and store or send to a insecure location the profile to disk. 
Data in transit can be read even if TLS secured using proxy or man in the middle. 

 */
var profile = new UserProfile()
{
    Email = "My@email.com",
    Name = "Jo Coder",
    DateOfBirth = new DateTime(2001, 7, 16),
    Devices = [IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.14"), IPAddress.Loopback]
};

var json = JsonConvert.SerializeObject(profile, options);
var fileName = Path.GetTempFileName();

//use inversion of control and generate a ILogger without dependency injection
Inverse.GetLogger("MyConsoleApp")?.LogInformation("Cyphered json:\n{json}", json);


await File.WriteAllTextAsync(fileName, json).ConfigureAwait(false);

//... rest of your code 


/*
 Read the json back in to a class using this simple extension method
 */
var cypheredJson = await File.ReadAllTextAsync(fileName).ConfigureAwait(false);

//use string extension method from Walter NuGet package to generate json from a string
if (cypheredJson.IsValidJson<UserProfile>(options, out UserProfile? user))
{
    //... user is not null and holds decrypted values as the console will show
    Inverse.GetLogger("MyConsoleApp")?.LogInformation("Profile:\n{profile}", user.ToString());
}

if (File.Exists(fileName))
{

    File.Delete(fileName);
}

```

## Encrypted UserProfile JSON Representation
In the example provided earlier, we showcased the serialization of a UserProfile object into a JSON format using advanced encryption techniques. This approach ensures that sensitive information within the UserProfile is securely obfuscated, requiring SHA-256 (or stronger) decryption capabilities for access. The JSON output is fully encrypted, making it a robust solution for protecting personal data, especially when adhering to GDPR standards or when data security is paramount.

Below is an example of how the UserProfile data appears after encryption and serialization into JSON. Note that each field are represented by a key (e.g., "a", "b", "c", "d"), and their values are encrypted strings. This encryption not only protects the data during transit or storage but also ensures that any identifiable information is not directly exposed in the serialized format.
```json
 {"a":"MyXKKzy8oKLeS1at6f5r7Ew1ZhL/9XpQFbQih6qC6fs=","b":"/XmXqpoYWyh9P/dimhCI46rAjCYQLdGdDJEoJLB9nnk=","c":"/LMLvcDRsS/WyuPw6yvFy67+jLsGsFWH07IxfFk1A7sKlQmgtZZQQF7E9743wxa2","d":["3CSNib1uly38gqAa15P+bRpl2aLG4HKf8D29OUEr3SI=","GSnPjkfS8zkSgmfOVT680dXf+fOWZR4pehyPOB0OnuM=","ryhts2Fn9jC0FUCfyRNVMp6ChNpa3t3jzojxpXLt0/o="]}
  
```
