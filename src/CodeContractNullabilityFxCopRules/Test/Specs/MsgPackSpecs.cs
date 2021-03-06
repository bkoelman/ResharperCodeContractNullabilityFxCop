﻿using System.IO;
using CodeContractNullabilityFxCopRules.ExternalAnnotations;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace CodeContractNullabilityFxCopRules.Test.Specs
{
    [TestFixture]
    internal class MsgPackSpecs
    {
        [Test]
        public void When_using_embedded_serializer_resource_it_must_roundtrip()
        {
            // Arrange
            var initialObject = new SerializerTestObject { Name = "Some" };
            var service = new MsgPackSerializationService();

            // Act
            using (var stream = new MemoryStream())
            {
                service.WriteObject(initialObject, stream);

                stream.Seek(0, SeekOrigin.Begin);

                var finalObject = service.ReadObject<SerializerTestObject>(stream);

                // Assert
                finalObject.Should().NotBeNull();
                finalObject.Name.Should().Be(initialObject.Name);
            }
        }
    }

    public class SerializerTestObject
    {
        [CanBeNull]
        public string Name { get; set; }
    }
}