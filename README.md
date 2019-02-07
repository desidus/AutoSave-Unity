# AutoSave for Unity
AutoSave plugin for Unity.

## Features
- AutoSave the changes you made to your scenes in the Editor.
- AutoSave scenes when Play button is pressed.
- Confirmation popup to perform or skip AutoSave. 
- Change the settings from the Preferences window.
- Works with multiscene.
- Works while the Unity Editor is running in background.
- Enable/Disable activity logs.

## How does it work
![](https://i.imgur.com/EfeHtee.png)

- **Enable AutoSave** switches on and off the plugin.
- **Save on Play** lets you save the open scenes when you press Play in the Editor.
- **Run while Editor in background** executes the plugin while the Editor window is not focused. [This forces the whole Editor to run in background. Turn it off if you expecience performance loss on your pc]
- **Show AutoSave popup** AutoSave shows a confirmation popup before saving.
- **Popup timeout in seconds** how many seconds before the popup is closed and the scenes saved.
- **AutoSave every # of minutes** is the minutes that have to pass between AutoSaves.

![](https://i.imgur.com/SfZT1LC.png)

## Install
Just import the asset from the AssetStore and you're good to go.
By default every function is enabled on the first start.

## Advanced setup
If you want to change the intervals maximum values, you must change them from the source code inside AutoSave.cs.

![](https://i.imgur.com/SzHOKr5.png)

## **Bonus** DesidusEditorUtility 
This plugin includes the DesidusEditorUtility class that can be used and called inside Editor scripts.
### Features
- **EditBoundsButton()** draws an edit button like the one used for colliders inspectors.
- **DisabledScriptField()** draws the field usually found at the top of MonoBehaviour inspectors with the component's class name.
- **GetEditorMainWindowPos()** returns the Rect position of the main editor window.
- **NavMeshAreaMaskField()** draws a mask field to select NavMesh areas, like the one found in the NavMeshAgent inspector.

## Built with
[Unity](https://www.unity3d.com)

## Authors
[Desidus](https://www.desidus.it) - [Contact us](info@desidus.it)

[Ettore Passaro](https://github.com/ximera91)

## License
This project is licensed under the MIT License. See the [LICENSE.md](LICENSE.md) file for details.
