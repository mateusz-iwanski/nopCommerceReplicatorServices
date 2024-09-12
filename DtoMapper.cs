using Azure.Identity;
using NLog;
using nopCommerceReplicatorServices.Exceptions;
using nopCommerceWebApiClient.Interfaces;
using nopCommerceWebApiClient.Objects.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices
{
    /// <summary>
    /// Represents a DTO mapper interface.
    /// </summary>
    public interface IDtoMapper
    {
        /// <summary>
        /// Maps the properties from the source DTO to the target DTO using a property dictionary.
        /// </summary>
        /// <typeparam name="TDtoToUpdate">The type of the target DTO.</typeparam>
        /// <typeparam name="TDtoSource">The type of the source DTO.</typeparam>
        /// <param name="dtoSource">The source DTO object.</param>
        /// <param name="propertyDictionary">The dictionary containing the properties to be mapped.</param>
        /// <returns>The target DTO object with the mapped properties.</returns>
        TDtoToUpdate Map<TDtoToUpdate, TDtoSource>(TDtoSource dtoSource, Dictionary<string, object> propertyDictionary)
            where TDtoToUpdate : IDto, new()
            where TDtoSource : IDto, new();
    }

    /// <summary>
    /// Represents a DTO custom mapper implementation.
    /// Maps the properties from the source DTO to the target DTO using a property dictionary.
    /// </summary>
    /// <remarks>
    /// For example: If ProductDto is marked as Type ProductDto and ProductUpdateBlockInformationDto is also marked as
    /// ProductDto, then the mapper will map the properties from ProductDto to ProductUpdateBlockInformationDto.
    /// When Map is called, you can set properties (propertyDictionary) with values that should be updated, while the rest of the data
    /// is copied from the source DTO.
    /// The target DTO (TDtoToUpdate) must have properties that are present in the source DTO (TDtoSource).
    /// </remarks>
    public class DtoMapper : IDtoMapper
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DtoMapper"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public DtoMapper(ILogger logger)
        {
            _logger = logger;
            return;
        }

        /// <inheritdoc/>
        public TDtoToUpdate Map<TDtoToUpdate, TDtoSource>(TDtoSource dtoSource, Dictionary<string, object> propertyDictionary)
            where TDtoToUpdate : IDto, new()
            where TDtoSource : IDto, new()
        {
            TDtoToUpdate dtoToMap = new TDtoToUpdate();

            if (dtoToMap.Type != dtoSource.Type)
                throw new Exceptions.TypeAccessException($"{nameof(dtoToMap)} is a different type than {nameof(dtoSource)}");

            PropertyInfo[] properties = typeof(TDtoToUpdate).GetProperties();

            var clonedData = CloneData<TDtoToUpdate, TDtoSource>(dtoSource, dtoToMap);

            // copy the properties from the dictionary to the cloned object (target object)
            foreach (var key in propertyDictionary.Keys)
            {
                PropertyInfo property = properties.FirstOrDefault(p => p.Name == key);
                // check if property exists in the target object
                if (property != null)   
                {
                    object value = propertyDictionary[key];
                    property.SetValue(clonedData, value);
                }
                else
                {                    
                    throw new CustomException($"Property {key} does not exist on the target object - {typeof(TDtoToUpdate).Name}");
                }
            }

            return clonedData;
        }

        /// <summary>
        /// Clones the data from the source DTO to the target DTO.
        /// </summary>
        /// <typeparam name="TDtoToUpdate">The type of the target DTO.</typeparam>
        /// <typeparam name="TDtoSource">The type of the source DTO.</typeparam>
        /// <param name="dtoSource">The source DTO object.</param>
        /// <param name="dtoToMap">The target DTO object.</param>
        /// <returns>The target DTO object with the cloned data.</returns>
        private TDtoToUpdate CloneData<TDtoToUpdate, TDtoSource>(TDtoSource dtoSource, TDtoToUpdate dtoToMap)
        {
            PropertyInfo[] targetProperties = dtoToMap.GetType().GetProperties();
            PropertyInfo[] sourceProperties = dtoSource.GetType().GetProperties();

            foreach (var targetProperty in targetProperties)
            {
                try
                {
                    // Skip properties without setters
                    if (!targetProperty.CanWrite) continue;

                    // Find the matching property in the dtoSource object
                    // The property must have the same name and type.
                    // If target property is nullable, then the source property can be non-nullable
                    // If source property is nullable, then the target property can't be non-nullable
                    var sourceProperty = sourceProperties
                        .FirstOrDefault(p => p.Name == targetProperty.Name &&
                            (p.PropertyType == targetProperty.PropertyType) ||
                            (p.PropertyType == Nullable.GetUnderlyingType(targetProperty.PropertyType)));
                    ;
                    if (sourceProperty == null)
                    {
                        throw new CustomException($"Property '{targetProperty.Name}' does not exist or type mismatch on dtoSource type '{dtoSource.GetType()}'. " +
                            "If source property is nullable, then the target property can't be non-nullable ");
                    }

                    object? value = sourceProperty.GetValue(dtoSource);
                    targetProperty.SetValue(dtoToMap, value);
                }
                catch (Exception ex)
                {
                    throw new CustomException(ex.Message, ex);
                }
            }

            return dtoToMap;
        }
    }
}
