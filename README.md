# AutoSaveBackup
Auto-save integration for Unity Editor with scene backup manager.

## Features
- Auto-save the changes you made to your scenes in the Editor.
- Auto-save scenes when Play button is pressed.
- Make scene backups and automatically delete them after a fixed time.
- Change the settings from the Preferences window.
- Works with multiscene.
- Works while the Unity Editor is running in background.
- Enable/Disable activity logs.

## How does it work
![alt text](http://i.imgur.com/A5eIFNR.png)

The six toggles activate/deactivate each function of the integration, but, if **Enable AutoSave** is off, the whole integration will be stopped.

The **AutoSave folder** must be inside the project folder or any of its subfolders.

- **AutoSave interval** is the number of minutes that should pass between each AutoSave. It must be a value between 1 and 15.
- **Scene backup interval** is the number of minutes that should pass between each scene backup. It must be a value between 1 and 30.
- **Delete backups interval** is the number of minutes that should pass before an old backup gets deleted. It must be a value between 1 and 90.

**Clean backup folder** button removes all the scene backups in the AutoSaves folder.

## Install
Just import the asset from the AssetStore and you're good to go.
By default every function is enabled on the first start.

## Advanced setup
If you want to change the intervals maximum values, you must change them from the source code inside AutoSave.cs.
![alt text](http://i.imgur.com/DL2gSPP.png)

## Built with
[Unity](https://www.unity3d.com)

## Authors
[GameBang](http://www.gamebang.it) - [Contact us](info@gamebang.it)

## License
This project is licensed under the MIT License. See the [LICENSE.md](LICENSE.md) file for details.