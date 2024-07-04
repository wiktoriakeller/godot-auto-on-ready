namespace GodotSourceGenerators.Console;

public class BaseItem
{
    public virtual void _Ready()
    {
        System.Console.WriteLine("Im ready!");
    }

    public T GetNode<T>(string path) where T : class, new()
    {
        System.Console.WriteLine(path);
        return new T();
    }
}
