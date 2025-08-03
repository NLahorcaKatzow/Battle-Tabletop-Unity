using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public ResourceController resourceController;

    public override void InstallBindings()
    {
        Container.Bind<ResourceController>().FromInstance(resourceController).AsSingle();
    }
}