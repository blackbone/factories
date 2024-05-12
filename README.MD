# FACTORIES

Simple package which allow to use power of source generators to handle factories pattern.

## Installation

Add `"com.blacbone.factories": "https://github.com/blackbone/factories.git"` to `manifest.json`

## Usage

Use `FactoryAttribute` on types which will be created from factories like so:

```csharp
[Factory(typeof(MyBaseClass), "key"]
[Factory(typeof(MyBaseClass), 123]
[Factory(typeof(MyBaseClass), Enum.EnumValue]
class MyClass : MyBaseClass
{
    // .. 
}

```

Each use of attribute with different combination of base type and key type will lead to registration in different factory.

To create instance call `Factory.Create()` where needed like so:

```csharp
void Foo()
{
    var instance1 = Factory<MyBaseClass, string>.Create("key");
    var instance2 = Factory<MyBaseClass, int>.Create(123);
    var instance3 = Factory<MyBaseClass, string>.Create(Enum.EnumValue);
}
```

In all three cases above instance will be of type MyClass.

## Issues and contribution

Feel free to create issues and send PR's.
There's no strict requirements.