# BitBench

## Introduction

BitBench is a proof of concept for a early Windows style UI for the [Commander X16](https://www.commanderx16.com/).

It features Process Separation and Management, Memory Management, Windowing System and Events. It does not include a control toolkit for creating UIs.

BitBench has been designed to either act as a operating system, or as a window UI for a single application.

It was written using [BitMagic:](https://github.com/Yazwh0/BitMagic) *a compiler, debugger and emulator for Windows and Linux*, as a form of [dog-fooding](https://en.wikipedia.org/wiki/Eating_your_own_dog_food).

## Process

Each process has a 'base' ram bank which it can be loaded into. The first few bytes of the bank define how the process should called, as well as jump vectors which can be called when various events take place.

As the X16 only has one processor with no capability for any type of process isolation, BitBench uses simple CPU time slicing to manage the difference processes.

Each VSYNC interrupt the scheduler looks at the processes that are active, and decides which to switch to.

A process sets how many frames it should be called between 1 and 255. If it sets this value to 0, it is designated a 'real time' process and will be called before any processes which request a update every frame using a value of 1.

Once a process is done with its time allocation it can hand back control for another process to be switched to.

If it doesn't hand back control, it will be interrupted and control handed to another process.

The stack and a segment of the Zero Page from 0x22 will be preserved, so will not change from the point of view of the process. The number of bytes from 0x22 can be set by the application in its header.

If the process needs to interact with the kernel or perform other IO operations it should disable the interrupts so that the application can be sure it is not switched out and essential kernel values are not changed.

BitBench supports up to 15 applications running at one time.

There is no current way to close an application.

## Memory Management

Memory management in BitBench is by RAM blocks within the 0x400 -> 0x9e00 range, as well as RAM banks from bank 0x01 upwards.

An application can request memory via the API. For banks this is one at a time, but for blocks it can request a number of continuous blocks.

## Windows

An application can create as many windows as it wants, however there is a limit to 16 windows at one time.

Events are currently limited, but arrive via a callback that is defined in the application's header. These are driven from the interrupt so will arrive while the application is running elsewhere.

The API allows the application to update data from a given x,y coordinate using tbe data structure that will be uploaded into VRAM.

Currently a second API call is needed to refresh the window's contents.

There is no current way to close a window, or add extra control icons.

## BitMagic

This project was developed to shake down the BitMagic development system.

Because BitMagic has a essentially limitless 'macro' capabilities, we can build a framework to develop the BitBench system.

The first part of this is the 'initialisation' library, which lets the referenced libraries call macros to define functionality. This lets us write the functionality which will be included in the `.prg` or `.bin` files, as well as ensure the segments, scopes and variables are setup correctly. As the developer controls this process, there should be no mystery in how this is performed.

The definition of the variables have a variable type associated with them. While this isn't used by the compiler, it is used by the debugger. So if a location is RAM is set to be a `ushort`, then displaying it in the watch window will use the correct format for a two byte value, greatly enhancing the debugging experience.

BitMagic's compiler is enhanced by source code reuse (rather than linking in a binary) which lets us include or exclude functionality based on what we need. For example in BitBench the window handler registers a callback for the mouse functionality. As this is performed in the 'macro' code before the asm is generated, it means if mouse support isn't needed the whole call chain is dropped from the final binary. While not useful for BitBench, its a good example of how source code reuse can produce tighter applications compared to the traditional linked assemblers.

Strong use of scopes has been used to ensure variables have good logical separation. These scopes also offer a way of creating an API for the calling applications.

Additionally, the whole of the generated code can be viewed within VSCode.

BitMagic is available via nuget, which means [Unit Tests](https://github.com/Yazwh0/BitBench/tree/main/tests/BenchTests) can be written to test the individual libraries. An example unit test project is included in the repository. These tests include testing for [side effects](https://github.com/Yazwh0/BitBench/blob/main/tests/BenchTests/Scheduler/OneNormal.cs#L69) for a complete test.

## Improvements

As a proof of concept, there are significant pieces of functionality missing.

- Processes cannot be terminated.
- Windows cannot be closed.
- The window updating mechanism is unnecessarily slow.
- Events should only have one call back vector so we're not constrained.
- Need a custom character set that features lowercase as well as inverted tiles.
- Windows should have icons for closing or other operations.
- Windows cannot currently be resized.
- No form of modal (messagebox) windows.
- Create a UI toolkit for buttons, textboxes, etc.
