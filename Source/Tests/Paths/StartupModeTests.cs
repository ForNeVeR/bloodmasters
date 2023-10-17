namespace CodeImp.Bloodmasters.Tests.Paths;

public sealed class StartupModeTests
{
    [Fact(DisplayName = "StartupMode should be Production when paths created")]
    public void StartupModeShouldBeProductionWhenPathsCreated()
    {
        // Arrange
        var sut = Bloodmasters.Paths.Create(StartupMode.Production);

        // Assert
        Assert.Equal(StartupMode.Production, sut.CurrentMode);
    }

    [Fact(DisplayName = "StartupMode should be Dev when paths created")]
    public void ShouldCreateProductionPathsWhenProductionModePassed()
    {
        // Arrange
        var sut = Bloodmasters.Paths.Create(StartupMode.Dev);

        // Assert
        Assert.Equal(StartupMode.Dev, sut.CurrentMode);
    }
}
