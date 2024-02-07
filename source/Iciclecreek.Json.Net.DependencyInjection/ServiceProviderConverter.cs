using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;

namespace Iciclecreek.Json.Net.DependencyInjection
{
    public class ServiceProviderConverter : JsonConverter
    {
        private readonly IServiceProvider _serviceProvider;
        private HashSet<Type> _dependencyTypes = new HashSet<Type>();
        private HashSet<Type> _notDependentTypes = new HashSet<Type>() { typeof(String) };

        // Constructor that takes an IServiceProvider as an argument
        public ServiceProviderConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Override the CanConvert method to check if the type is assignable from T
        public override bool CanConvert(Type objectType)
        {
            if (objectType.IsValueType || _notDependentTypes.Contains(objectType))
                return false;

            if (_dependencyTypes.Contains(objectType))
                return true;

            try
            {
                // attempt to create the object, if we can then we don't need dependency injection.
                Activator.CreateInstance(objectType);
                _notDependentTypes.Add(objectType);
                return false;
            }
            catch (MissingMethodException)
            {
                _dependencyTypes.Add(objectType);
                return true;
            }
        }

        // Override the ReadJson method to create an instance of T using the service provider and the JSON object
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Create an instance of T using the service provider and the JSON object as a constructor argument
            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, objectType);

            // Populate the instance with the JSON object
            serializer.Populate(reader, instance);

            // Return the instance
            return instance;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException("CustomCreationConverter should only be used while deserializing.");
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this <see cref="JsonConverter"/> can write JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanWrite => false;

    }

    public class ServiceProviderConverter<T> : CustomCreationConverter<T>
    {
        private readonly IServiceProvider _serviceProvider;

        // Constructor that takes an IServiceProvider as an argument
        public ServiceProviderConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override T Create(Type objectType)
        {
            // Create an instance of T using the service provider and the JSON object as a constructor argument
            return ActivatorUtilities.CreateInstance<T>(_serviceProvider);
        }
    }
}
