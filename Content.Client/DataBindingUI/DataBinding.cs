
using JetBrains.Annotations;

namespace Content.Client.DataBindingUI;

[PublicAPI]
public sealed class ContextBindingExtension
{
    public string Key { get; }

    public ContextBindingExtension(string key)
    {
        Key = key;
    }

    public object ProvideValue()
    {
        return new BindableData("/" + Key.Replace(" ","/"));
    }
}

public sealed class BindableData
{
    private readonly string _key;

    public BindableData(string key)
    {
        _key = key;
    }

    public T GetValue<T>() where T : class
    {
        var value = IoCManager.Resolve<IViewVariablesManager>().ReadPath(_key);
        
        if (value is T t)
            return t;
        
        throw new Exception();
    }
    
    public static implicit operator BindableData(string key) => new(key);
}