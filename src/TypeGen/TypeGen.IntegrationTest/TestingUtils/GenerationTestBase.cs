using System;
using System.Threading.Tasks;
using TypeGen.Core.Generator;
using TypeGen.Core.SpecGeneration;
using TypeGen.IntegrationTest.Extensions;
using Xunit;

namespace TypeGen.IntegrationTest.TestingUtils;

public class GenerationTestBase
{
    protected async Task TestFromAssembly(Type type, string expectedLocation)
    {
        var readExpectedTask = EmbededResourceReader.GetEmbeddedResourceAsync(expectedLocation);

        var generator = new Generator();
        var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);

        await generator.GenerateAsync(type.Assembly);
        var expected = (await readExpectedTask).Trim();

        Assert.True(interceptor.GeneratedOutputs.ContainsKey(type));
        Assert.Equal(expected, interceptor.GeneratedOutputs[type].Content.FormatOutput());
    }
    
    protected static async Task TestGenerationSpec(Type type, string expectedLocation,
        GenerationSpec generationSpec, GeneratorOptions generatorOptions)
    {
        var readExpectedTask = EmbededResourceReader.GetEmbeddedResourceAsync(expectedLocation);
        var generator = new Core.Generator.Generator(generatorOptions);
        var interceptor = GeneratorOutputInterceptor.CreateInterceptor(generator);

        await generator.GenerateAsync(new[] { generationSpec });
        var expected = (await readExpectedTask).Trim();

        Assert.True(interceptor.GeneratedOutputs.ContainsKey(type));
        Assert.Equal(expected, interceptor.GeneratedOutputs[type].Content.FormatOutput());
    }
}