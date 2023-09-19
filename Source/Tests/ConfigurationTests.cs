using System.Text;

namespace CodeImp.Bloodmasters.Tests;

public class ConfigurationTests
{
    [Fact]
    public async Task ConfigReadsFileWithBom()
    {
        var configFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(configFilePath, """host = "tratata";""",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        var configuration = new Configuration(configFilePath);
        Assert.Equal("tratata", configuration.ReadSetting("host", ""));
    }
}
