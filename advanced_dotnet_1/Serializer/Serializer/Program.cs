using System;
using System.Collections.Generic;
using System.IO;

namespace Serializer {

    /// <summary>
    /// Class for serializing object data
    /// </summary>
    /// <typeparam name="T">
    /// Serialized data type
    /// </typeparam>
    public class RootDescriptor<T> {
        public string RootName { get; set; }                           // Name of the root element
        
        private List<Item> _items = new List<Item>();                  // Collection of data items
        
        
        /// <summary>
        /// Abstract class representing one data item to be serialized
        /// </summary>
        private abstract class Item                                    
        {
            public string ElemName { get; set; }
            
            public abstract void Serialize(TextWriter writer, T instance);
        }

        /// <summary>
        /// Item of a basic type that does not require additional serialization
        /// </summary>
        /// <typeparam name="TR"> Type of the item </typeparam>
        private class BasicItem<TR> : Item
        {
            public Func<T, TR> Map { get; set; }
            public override void Serialize(TextWriter writer, T instance)
            {
                writer.WriteLine($"<{ElemName}>{Map(instance)}</{ElemName}>");
            }
        }

        /// <summary>
        /// Complex item that itself needs to be serialized and is provided with its own descriptor
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        private class ComplexItem<TR> : Item
        {
            public RootDescriptor<TR> Descriptor { get; set; }
            public Func<T, TR> Map { get; set; }

            public override void Serialize(TextWriter writer, T instance)
            {
                writer.WriteLine($"<{ElemName}>");
                Descriptor.SerializeInner(writer, Map(instance));
                writer.WriteLine($"</{ElemName}>");
            }
        }
        
        // Functions that add new items to be serialized
        // Return `this` to enable method chaining
        public RootDescriptor<T> Add(string name, Func<T, string> map)
        {
            _items.Add(new BasicItem<string>{ElemName = name, Map = map});
            return this;
        }

        public RootDescriptor<T> Add(string name, Func<T, int> map)
        {
            _items.Add(new BasicItem<int>{ElemName = name, Map = map});
            return this;
        }

        public RootDescriptor<T> Add<TR>(string name, Func<T, TR> map, RootDescriptor<TR> descriptor)
        {
            _items.Add(new ComplexItem<TR>{ElemName = name, Map = map, Descriptor = descriptor});
            return this;
        }

        /// <summary>
        /// Serializes an instance to XML
        /// </summary>
        public void Serialize(TextWriter writer, T instance)
        {
            writer.WriteLine($"<{RootName}>");
            SerializeInner(writer, instance);
            writer.WriteLine($"</{RootName}>");
        }
        
        // Serializes the object without including the root element
        // Useful for Descriptors nested inside other Descriptor elements
        private void SerializeInner(TextWriter writer, T instance)
        {
            foreach (var item in _items)
            {
                item.Serialize(writer, instance);
            }
        }
    }

    class Address {
        public string Street { get; set; }
        public string City { get; set; }
    }

    class Country {
        public string Name { get; set; }
        public int AreaCode { get; set; }
    }

    class PhoneNumber {
        public Country Country { get; set; }
        public int Number { get; set; }
    }

    class Person {
        public string FirstName { get; set; }
        public string LastName { get; set; }	
        public Address HomeAddress { get; set; }
        public Address WorkAddress { get; set; }
        public Country CitizenOf { get; set; }
        public PhoneNumber MobilePhone { get; set; }
    }

    class Program {
        static void Main() {
            RootDescriptor<Person> rootDesc = GetPersonDescriptor();
			
            var czechRepublic = new Country { Name = "Czech Republic", AreaCode = 420 };
            var person = new Person {
                FirstName = "Pavel",
                LastName = "Jezek",
                HomeAddress = new Address { Street = "Patkova", City = "Prague" },
                WorkAddress = new Address { Street = "Malostranske namesti", City = "Prague" },
                CitizenOf = czechRepublic,
                MobilePhone = new PhoneNumber { Country = czechRepublic, Number = 123456789 }
            };

            rootDesc.Serialize(Console.Out, person);
        }

        static RootDescriptor<Person> GetPersonDescriptor() {
            var rootDesc = new RootDescriptor<Person>{RootName = nameof(Person)};
            var addrDesc = GetAddressDescriptor();
            var cntryDesc = GetCountryDescriptor();
            var phoneDesc = GetPhoneNumberDescriptor();
            return rootDesc.Add(nameof(Person.FirstName), t => t.FirstName)
                    .Add(nameof(Person.LastName), t => t.LastName)
                    .Add(nameof(Person.HomeAddress), t => t.HomeAddress, addrDesc)
                    .Add(nameof(Person.WorkAddress), t => t.WorkAddress, addrDesc)
                    .Add(nameof(Person.CitizenOf), t => t.CitizenOf, cntryDesc)
                    .Add(nameof(Person.MobilePhone), t => t.MobilePhone, phoneDesc);
        }

        static RootDescriptor<Address> GetAddressDescriptor()
        {
            var rootDesc = new RootDescriptor<Address>{RootName = nameof(Address)};
            return rootDesc.Add("Street", t => t.Street)
                    .Add("City", t => t.City);
        }

        static RootDescriptor<Country> GetCountryDescriptor()
        {
            var rootDesc = new RootDescriptor<Country>{RootName = nameof(Country)};
            return rootDesc.Add(nameof(Country.Name), t => t.Name)
                    .Add(nameof(Country.AreaCode), t => t.AreaCode);
        }

        static RootDescriptor<PhoneNumber> GetPhoneNumberDescriptor()
        {
            var rootDesc= new RootDescriptor<PhoneNumber>{RootName = nameof(PhoneNumber)};
            var countryDesc = GetCountryDescriptor();
            return rootDesc.Add(nameof(PhoneNumber.Country), t => t.Country, countryDesc)
                    .Add(nameof(PhoneNumber.Number), t => t.Number);
        }
    }
}
