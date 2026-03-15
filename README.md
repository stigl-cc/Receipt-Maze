# Receipt Maze Generator
  Maze generator that outputs in ESC/POS (or alternatively PPM4) receipt printer format.  
  The generator utilizes the Depth First Search algorithm making it both simple and performant.  

## Options
  `--help`               Get Help  
  `--width/-w <number>`  Width of the maze grid (not of the resulting image) [default = 42]  
  `--height/-h <number>` Height of the maze grid (not of the resulting image) [default = 64]  
  `--scale/-s <number>`  Pixels per maze grid cell [default = 13]  
  `--seed <number>`      Seed for maze generation (makes for reproducible mazes) [default = current unix ms]  
  `--revisit/-r`         Allows revisiting cells, leading to imperfect mazes and mazes with loops [default = false; flag by itself enables it]  
  `--ppm4/-p`            Enables netpbm output instead of ESC/POS for receipt printers [default = escpos; flag by itself makes it netpbm]

## Running
  Note: You will need dotnet sdk. the project uses dotnet core 9 by default, but dotnet core 6 onward will likely work aswell.  
  Since the program is written in C#, you can simply run it with `dotnet run`.  
  To then redirect its output to a receipt printer, you can pipe it as so: `dotnet run > /dev/usb/lp0`.  
  When saving to a netpbm, you can use `dotnet run -- -p > image.ppm`.  

## Example
![Maze Example](images/image.png)
