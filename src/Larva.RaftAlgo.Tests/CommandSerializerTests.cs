using System;
using System.Text;

namespace Larva.RaftAlgo.Tests
{
    public class CommandSerializerTests
    {
        public CommandSerializerTests()
        {

        }

        [Xunit.Fact]
        public void Test1()
        {
            var command = new GenerateBusinessCodeCommand(new Guid("902e7bc8-0754-48b0-8396-ebebb802257c"), 1, new DateTime(2020, 8, 9, 22, 0, 0));
            var serializer = new JsonCommandSerializer();
            serializer.Initialize(typeof(CommandSerializerTests).Assembly);

            var commandData = serializer.Serialize(command.GetType(), command);
            var json = Encoding.UTF8.GetString(commandData.data);
            Console.WriteLine(json);

            var command2 = serializer.Deserialize(commandData.typeName, commandData.data);
        }
    }
}