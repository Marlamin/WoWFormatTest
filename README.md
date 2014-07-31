# WoW Format Test
Messing around with parsing WoW file formats for educational purposes.
Built from the ground up, but with lots of copy pasta from older projects (noted in thanks paragraph).

## Supported expansions
Built for (latest) Warlords of Draenor files. Backwards compatibility with Mists of Pandaria not guaranteed. Cataclysm and lower probably won't work at all. 
Files need to be extracted to the HDD with intact folder structures. MPQ support is currently not planned. Support for the new CASC file format introduced in Warlords of Draenor might be included in the future. 

## Configuration
The App.config file in the WoWFormatUI project must be edited to point to the location of the extracted files.

## Requirements
- [CSDBCReader](http://marlamin.com/u/CSDBCReader.dll)
- [SharpDX 2.6.2](http://sharpdx.org/download/) (Should be downloaded automatically through NuGet)
- [SharpDX.WPF](https://github.com/Marlamin/SharpDX.WPF) (Updated version for SharpDX 6.x)

## Thanks
- TOM_RUS
- flippy84
- JustMak
- Xalcon
- BoogieMan 
- WoWDev wiki authors (especially schlumpf!!!!)
- Miceiken
- ..and all the people the above people base their work on
- also Thoorium

