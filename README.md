# ScriptableEnum

`ScriptableEnum` is a Unity package that provides a flexible and extensible way to create and manage enumerations as ScriptableObjects. This allows for dynamic and customizable enums that can be easily modified and extended without requiring code changes.

## Features

- **Dynamic Enums**: Create enums as ScriptableObjects, allowing for runtime modifications.
- **Editor Integration**: Full support for Unity's Editor, including custom property drawers and UI elements.
- **Type-Safe**: Generic support for type-safe enums.
- **Searchable**: Easily search and filter enums in the Unity Editor.
- **Customizable Display Text**: Each enum value can have a custom display text.
- **Ping and Open in Editor**: Quickly locate and edit enum assets directly from the Inspector.

## Installation

1. Download the package or clone the repository.
2. Place the `ScriptableEnums` folder into your Unity project's `Assets` directory.
3. Ensure the package is imported correctly in Unity.

## Usage

### Creating a ScriptableEnum

1. Create a new class that inherits from `ScriptableEnum` or `ScriptableEnum<T>`.
2. Define your enum values as ScriptableObject instances in the Unity Editor.

```csharp
using UnityEngine;
using Tauntastic.ScriptableEnums;

[CreateAssetMenu(fileName = "NewColorEnum", menuName = "Enums/ColorEnum")]
public class ColorEnum : ScriptableEnum<ColorEnum>
{
    // Custom properties or methods can be added here
}
```
Using ScriptableEnum in Code
```csharp
public class Example : MonoBehaviour
{
    public ColorEnum color;

    private void OnValidate()
    {
        // Access all options of the enum
        var allColors = ColorEnum.AllOptions;

        // Get a specific enum value by name
        var redColor = ColorEnum.GetByName("Red");

        // Use the enum value
        Debug.Log($"Selected color: {color.DisplayText}");
    }
}
```
Using [ScriptableEnum] Attribute
You can use the [ScriptableEnum] attribute to directly reference a ScriptableEnum in your MonoBehaviour or ScriptableObject.

csharp
Copy
public class PlayerSettings : MonoBehaviour
{
    [ScriptableEnum]
    public ColorEnum playerColor;

    private void OnValidate()
    {
        Debug.Log($"Player color: {playerColor.DisplayText}");
    }
}
Example: Using ScriptableEnum with a ScriptableObject
csharp
Copy
using UnityEngine;
using Tauntastic.ScriptableEnums;

[CreateAssetMenu(fileName = "NewColorSettings", menuName = "Settings/ColorSettings")]
public class ColorSettings : ScriptableObject
{
    [ScriptableEnum]
    public ColorEnum ColorSO;

    private void OnValidate()
    {
        Debug.Log($"Selected color: {ColorSO.DisplayText}");
    }
}
API Reference
ScriptableEnum
DisplayText: Gets or sets the display text for the enum value.

GetAllOptions<T>(): Returns all instances of a specific ScriptableEnum type.

GetAllOptions(Type type): Returns all instances of a specific ScriptableEnum type by Type.

ScriptableEnum<T>
AllOptions: Returns all instances of the specific ScriptableEnum<T> type.

GetByName(string textIdentifier): Retrieves an enum instance by its display name.

GetAllInstances(): Returns all instances of the specific ScriptableEnum<T> type.

Attributes
ScriptableEnumAttribute: Marks a field or property as a ScriptableEnum.

DisableAttribute: Disables a field or property in the Inspector.

Examples
Example 1: Creating a Color Enum
csharp
Copy
[CreateAssetMenu(fileName = "NewColorEnum", menuName = "Enums/ColorEnum")]
public class ColorEnum : ScriptableEnum<ColorEnum>
{
    // Custom properties or methods can be added here
}
Example 2: Using ScriptableEnum in a MonoBehaviour
csharp
Copy
public class Player : MonoBehaviour
{
    public ColorEnum playerColor;

    private void OnValidate()
    {
        Debug.Log($"Player color: {playerColor.DisplayText}");
    }
}
Example 3: Using [ScriptableEnum] with a ScriptableObject
csharp
Copy
[CreateAssetMenu(fileName = "NewColorSettings", menuName = "Settings/ColorSettings")]
public class ColorSettings : ScriptableObject
{
    [ScriptableEnum]
    public ColorEnum ColorSO;

    private void OnValidate()
    {
        Debug.Log($"Selected color: {ColorSO.DisplayText}");
    }
}
License
This project is licensed under the MIT License. See the LICENSE file for details.

Contributing
Contributions are welcome! Please open an issue or submit a pull request for any bugs, features, or improvements.

Support
If you encounter any issues or have questions, please open an issue on the GitHub repository.

Note: This package is designed for Unity 2020.3 LTS or later. Ensure your project meets the minimum requirements.
