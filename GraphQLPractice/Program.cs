using GraphQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Builders;
using GraphQL.Http;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQLPractice
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            Console.WriteLine("Hello GraphQL!");

            ShowResult(await ExecuteSchema(
                new Schema() { Query = new StarWarsQuery() },
                @"
                query {
                  hero {
                    id
                    name
                  },
                  droid {
                    id
                    name
                  }
                }
                "));

            ShowResult(await ExecuteSchema(
                new Schema() { Query = new StarWarsQuery() },
                @"
                query {
                  droid(id: ""1"") {
                    id
                    name
                  }
                }
                "));

            ShowResult(await ExecuteSchema(
                new Schema() { Query = new StarWarsQuery() },
                @"
                query($droid: DroidInput) {
                  droid(data: $droid) {
                    id
                    name
                  }
                }
                ",
                @"
                {
                    ""droid"": {
                      ""id"": ""2""
                    }
                }
                ".ToInputs()));

            ShowResult(await ExecuteSchema(
                new Schema() { Mutation = new StarWarsMutation() },
                @"
                mutation ($droid: DroidInput) {
                  createDroid (data: $droid) {
                    id
                    name
                  }
                }
                ",
                @"
                {
                    ""droid"": {
                      ""id"": ""3"",
                      ""name"": ""Test""
                    }
                }
                ".ToInputs()));

            ShowResult(await ExecuteSchema(
                new StarWarsSchema(),
                @"
                mutation ($droid: DroidInput) {
                  createDroid (data: $droid) {
                    id
                    name
                  }
                }
                ",
                @"
                {
                    ""droid"": {
                      ""id"": ""3"",
                      ""name"": ""Test""
                    }
                }
                ".ToInputs()));
        }

        private static void ShowResult(ExecutionResult result)
        {
            var json = new DocumentWriter(indent: true).Write(result);
            Console.WriteLine(json);
        }

        private static async Task<ExecutionResult> ExecuteSchema(Schema schema, string query, Inputs inputs = null)
        {
            var options = new ExecutionOptions
            {
                Schema = schema,
                Query = query,
                Inputs = inputs
            };
            var result = await new DocumentExecuter()
                .ExecuteAsync(options)
                .ConfigureAwait(false);
            return result;
        }
    }

    public class Droid
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class DroidType : ObjectGraphType<Droid>
    {
        public DroidType()
        {
            Field(x => x.Id).Description("The Id of the Droid.");
            Field(x => x.Name, nullable: true).Description("The name of the Droid.");
        }
    }

    public class DroidInputType : InputObjectGraphType
    {
        public DroidInputType()
        {
            Name = "DroidInput";
            Field<NonNullGraphType<StringGraphType>>("id");
            Field<StringGraphType>("name");
        }
    }

    public class Hero
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class HeroType : ObjectGraphType<Hero>
    {
        public HeroType()
        {
            Field(x => x.Id).Description("The Id of the hero.");
            Field(x => x.Name, nullable: true).Description("The name of hero.");
        }
    }

    public class StarWarsSchema : Schema
    {
        public StarWarsSchema()
        {
            Query = (StarWarsQuery)ResolveType(typeof(StarWarsQuery));
            Mutation = (StarWarsMutation)ResolveType(typeof(StarWarsMutation));
        }
    }

    public class StarWarsQuery : ObjectGraphType
    {
        public StarWarsQuery()
        {
            Field<ListGraphType<DroidType>>()
                .Name("droid")
                .Argument<DroidInputType>("data", "Droid")
                .Argument<StringGraphType>("id", "The id of droid")
                .Resolve(context =>
                {
                    var filterId = context.GetArgument<string>("id");
                    var filterDroidData = context.GetArgument<Droid>("data");

                    return new List<Droid>()
                        {
                            new Droid {Id = "1", Name = "R2-D2"},
                            new Droid {Id = "2", Name = "C-3PO"}
                        }
                        .Where(droid => (filterId != null && droid.Id == filterId) || filterId == null)
                        .Where(droid =>
                            (filterDroidData != null && droid.Id == filterDroidData.Id) || filterDroidData == null);
                });

            Field<HeroType>()
                .Name("hero")
                .Resolve(context =>
                {
                    Console.WriteLine("Hero");
                    return new Hero() { Id = "1", Name = "JEDI" };
                });
        }
    }

    public class StarWarsMutation : ObjectGraphType
    {
        public StarWarsMutation()
        {
            Field<ListGraphType<DroidType>>()
                .Name("createDroid")
                .Argument<DroidInputType>("data", "Droid")
                .Resolve(context =>
                {
                    var droid = context.GetArgument<Droid>("data");

                    return new List<Droid>()
                    {
                        new Droid {Id = "1", Name = "R2-D2"},
                        new Droid {Id = "2", Name = "C-3PO"},
                        new Droid {Id = droid.Id, Name = droid.Name}
                    };
                });
        }
    }
}