# Console Renderer Engine
Old project from 3 years ago or so and decided to public it. Before putting this on GitHub, I did some code clean up so now minimum version is .NET 8 (it was 6 before).

There's some unused/commented out code, but I didn't remove it because I do not remember what I was trying to do.

# Functionality
From what I recall:
* You can layer pixels, meaning if you move some pixel from layer 1, layer 0 then show up.
* You can swap pixel location with another pixel.
* Text renderer.
* Debug mode that I don't know if it even works still (shows FPS counter and I hope it's correctly implemented).
* Each pixel takes up two letters in the buffer, the background color is just changed.
* This is not fully a game renderer because you still have to make some threads for moving pixels (like I did in snake game example).
* But yeah I did not test this enough, now you are on your own, back then I was just messing with the terminal capabilities and experimenting.
* Keep in mind that not all methods are tested out, I made them out of convenience because I was thinking eventually publish this, but I never finished second example, Tetris.

## Examples
They reside in separate project "Examples".

* Fully functional snake game (play with arrow keys).
  * Apple generation is random so no idea what happens when apple tries to spawn inside snake tail's pixel.
* Unplayable and unfinished Tetris game (pieces do fall but that's it).

## Screenshots

<img width="1146" height="648" alt="{EA3A4326-79BE-4EF8-9071-4F4EA9C78DEA}" src="https://github.com/user-attachments/assets/00c27b2b-eb20-4ac2-b19e-1d70bd5bb746" />

https://github.com/user-attachments/assets/12e10cb4-4147-4298-ac0b-04b7eb7174e4

