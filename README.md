# Zenject-Template

A **Unity** project template intended for projects that use **Zenject / Extenject** (dependency injection).

## What’s inside

- Standard Unity project layout: `Assets/`, `Packages/`, `ProjectSettings/`
- `Saves/` directory for local save data (optional)

## Getting started

1. Click **Use this template** (or fork/clone the repo).
2. Open the project folder in **Unity Hub**.
3. Open any scene and press **Play**.

## Using Zenject / Extenject (quick overview)

- Put app-wide bindings in **ProjectContext** (global services/singletons).
- Put scene-specific bindings in **SceneContext**.
- Register dependencies in **Installers**, then inject them via constructors/fields/methods.

Minimal example:

```csharp
using Zenject;

public interface IInputService { }

public sealed class InputService : IInputService { }

public sealed class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IInputService>().To<InputService>().AsSingle();
    }
}
```

## References

- Zenject (original): https://github.com/modesttree/Zenject
- Extenject (community-maintained fork): https://github.com/Mathijs-Bakker/Extenject

## License

Add a `LICENSE` file if you want to make the licensing explicit (e.g., MIT).
