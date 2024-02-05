using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Iciclecreek.Json.Net.DependencyInjection.Tests
{


    [TestClass]
    public class InjectionTests
    {

        [TestMethod]
        public void TestUniversal()
        {
            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<JsonSerializerSettings>((sp) => new JsonSerializerSettings() 
                { 
                    Converters = new List<JsonConverter>() 
                    { 
                        new ServiceProviderConverter(sp) 
                    } 
                })
                .AddSingleton<IConfiguration>((sp) => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    { "test","value"}
                }).Build())
                .BuildServiceProvider();

            var json = @"
{ 
    'Sub': { 
        'Name': 'Joe', 
        'Foo':{ 
            'x': 13
        }
    } 
}";
            
            var result = JsonConvert.DeserializeObject<TestClass>(json, serviceProvider.GetRequiredService<JsonSerializerSettings>());
            Assert.IsNotNull(result);
            Assert.AreEqual("Joe", result.Sub.Name);
            Assert.AreEqual(13, result.Sub.Foo.X);
            Assert.IsTrue(result.HasConfiguration);
            Assert.IsTrue(result.Sub.HasConfiguration);
        }

        [TestMethod]
        public void TestRegistered()
        {
            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<JsonSerializerSettings>((sp) => new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>() 
                    { 
                        new ServiceProviderConverter<TestClass>(sp),
                        new ServiceProviderConverter<SubClass>(sp)
                    }
                })
                .AddSingleton<IConfiguration>((sp) => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    { "test","value"}
                }).Build())
                .BuildServiceProvider();

            var json = @"
{ 
    'Sub': { 
        'Name': 'Joe', 
        'Foo':{ 
            'x': 13
        }
    } 
}";

            var result = JsonConvert.DeserializeObject<TestClass>(json, serviceProvider.GetRequiredService<JsonSerializerSettings>());
            Assert.IsNotNull(result);
            Assert.AreEqual("Joe", result.Sub.Name);
            Assert.AreEqual(13, result.Sub.Foo.X);
            Assert.IsTrue(result.HasConfiguration);
            Assert.IsTrue(result.Sub.HasConfiguration);
        }
    }

    public class Foo
    {
        public int X { get; set; }
    }


    public class SubClass
    {
        private readonly IConfiguration configuration;

        public SubClass(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string Name { get; set; }

        public Foo Foo { get; set; }

        public bool HasConfiguration => configuration != null;
    }

    public class TestClass
    {
        private readonly IConfiguration configuration;

        public TestClass(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public SubClass Sub { get; set; }

        public bool HasConfiguration => configuration != null;
    }

}