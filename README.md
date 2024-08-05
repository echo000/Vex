# Vex

Vex is an asset extractor for various titles running on the Void Engine.
Supported games are Dishonored 2, Death of the Outsider (WIP) & Deathloop.

This is a WIP - Currently supports extracting Models, Materials, Images and RawFiles.

Animations are WIP - The module that extracts them is currently hidden and won't be released until it's in a more stable state.

Note that a fresh build will not work due to the lack of animation module, you will need to comment out ExportVoidAnimation in VoidSupport.cs to get it to build.

Death of the outsider support is WIP - Some assets fail to extract properly due to an unknown compression format.

Note that SEModel/Anim plugins are not directly supported in Maya or Blender anymore, and would recommend using Cast.

Press P to preview Models/Textures in the viewer. (Note that models can take a while to load if loading with textures due to needing to build the textures)

## Requirements

* Windows 11 x64 or above (Windows 7/8/8.1/10 should work, but are untested)
* .NET Core 9 (Technically would build with .NET 8 but would require some changes)
* Official copies of the games (only the latest copies from official distributors are tested)
* General understanding of how to use the assets you want to work with

The following tools/plugins are required/recommended for some assets:

* [Cast](https://github.com/dtzxporter/Cast) by DTZxPorter (.cast) (Autodesk Maya/Blender)
* [SETools](https://github.com/dtzxporter/SETools) by DTZxPorter (.seanim & .semodel) (Autodesk Maya)
* [io_anim_seanim](https://github.com/SE2Dev/io_anim_seanim) by SE2Dev (.seanim) (Blender)
* [io_model_semodel](https://github.com/dtzxporter/io_model_semodel) by DTZxPorter (.semodel) (Blender)
* [FileTypeDDS](https://github.com/dtzxporter/FileTypeDDS) by DTZxPorter (support in Paint .NET for newer DXGI formats) (Paint .NET)
* [Intel TextureWorks](https://software.intel.com/en-us/articles/intel-texture-works-plugin) by Intel (DDS + Utils) (Photoshop)
* [SEModelViewer](https://github.com/Scobalula/semodelviewer) by Scobalula

## License/Disclaimers

Vex is licensed under the General Public License 3.0, you are free to use Vex, both it and its source code, under the terms of the GPL. Vex is distributed in the hope it will be useful to, but it comes WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE, see the LICENSE file for more information.

This repo is in no shape or form associated with Activision and the developers. These tools are developed to allow users access to assets for use in 3D art such as YouTube thumbnails, etc. The assets extracted by these tools are property of their respective owners and what you do with the assets is your own responsbility.

## Credits/Contributors

* [echo000](https://github.com/echo000) - Developer and Maintainer
* [Scobalula](https://github.com/Scobalula) - [PhilLibX](https://github.com/Scobalula/PhilLibX)
* [DTZxPorter](https://github.com/dtzxporter/) - Help with Research - Porter has been a HUGE help, this entire project wouldn't be possible without him.
* id-daemon - Game Research

**If you use Vex in any of your projects, it would be highly appreciated if you credit the people/parties listed in the Credits list.**
