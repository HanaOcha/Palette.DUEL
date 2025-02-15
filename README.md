# Palette.DUEL
A Palette creator for [Duelists of Eden](https://store.steampowered.com/app/1664200/Duelists_of_Eden/), a game by [Thomas Moon Kang](https://thomasmoonkang.com/)
![Screenshot 2025-02-13 024848](https://github.com/user-attachments/assets/3f21504f-ac39-4095-8f65-95788c9c02c6)

## Features
A full palette editor similar to [yert.pink](https://yert.pink/paletteeditor)
![Screenshot 2025-02-14 170935](https://github.com/user-attachments/assets/8e11610d-1ca9-43b9-8828-e01bc087a1bb)

- Create and edit each pixel of a palette file
- View the palette and its animation in real time
- Select colors directly from the sprite
- Color picker with HSV support (thanks to [PixiEditor/ColorPicker](https://github.com/PixiEditor/ColorPicker))
- Other colors become translucent when one is selected. Control it with the Focus Opacity bar.
- Swap between 3 different view modes:
  1. Base - Shows the base palette with no changes
  2. Preview - Shows the current edits of the palette
  3. Default - Shows the original palette of the character
- A "Find Next" button which finds the next available frame with the color you're currently editing
- Add Reference Images. Click anywhere on the image to use its color in the color picker
- All characters and outfits currently in the game

## Controls
```
1, 2, 3   Switches View Modes
A         Toggle Animation

.         Frame Forward
,         Frame Back
Ctrl+.    Frame Forward 10
Ctrl+,    Frame Back 10

I         Reset to Idle Frame
Ctrl+F    Find Next

O         Toggle Focus Opacity

Ctrl+S    Export
```
- Palette buttons have Right-Click functionality

### Known Bugs
- Known issue where a picture imported via Reference will not have its colors picked correctly
  - It will genuinely just be a random image specific to your machine
  - Copying or downloading the image from anywhere else fixes it
