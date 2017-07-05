# AutoSaveBackup
Auto-save integration for Unity Editor with scene backup manager.

## Features
- Auto-save the changes you made to your scenes in the Editor.
- Make scene backups and automatically delete them after a fixed time.
- Change the settings from the Preferences window.
- Works with multiscene.

## How does it work
![alt text](http://i.imgur.com/Rg9IIAx.png?1)

The three toggles activate/deactivate each function of the integration, but if Enable AutoSave is off the whole integration will be stopped.

The AutoSave folder must be inside the project folder or any of its subfolders.

- AutoSave Interval is the number of minutes that have to pass between each AutoSave. It must be a value between 1 and 15.
- Backup Interval is the number of minutes that have to pass between each scene backup. It must be a value between 1 and 30.
- Remove Interval is the number of minutes that have to pass before an old backup gets deleted. It must be a value between 1 and 90.

Clean Backup Folder button removes all the scene backups in the AutoSaves folder.

## Install
Just import the .unitypackage into your project.
By default every function is enabled on the first start.

## Advanced setup
If you want to change the intervals maximum values, you have to change them from the source code inside AutoSave.cs.
![alt text](http://i.imgur.com/ZsjLMp8.png)

## Built with
[Unity](https://www.unity3d.com)

## Authors
Ettore Passaro, game developer at [GameBang](http://www.gamebang.it)

## Licence
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details