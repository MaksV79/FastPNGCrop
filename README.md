# FastPNGCrop
Fastest way to crop many PNG files in one directory.

The main purpose of the program is to cut files without any dialogs to select the file name, confirm overwriting, etc. The selection area is saved for the following files, this is especially useful for cutting faces into a sequence of frames from the video.
ATTENTION! Before you start, make a copy of the folder with the files you want to crop! All changes are made without confirmation, and without possibility to cancel.
After starting the program, select the folder with PNG files. The first PNG file will appear.
 - To start selection area to crop, left mouse button click, specify the upper left corner, then the lower right corner.
 - To cut, press Enter. File will be cropped and written with the same name without confirmation. You will be switched to next file.
 - If you want to re-select the selection area, press Esc. 
 - If the frame does not need to be cropped and want to go to the next file, press right cursor key. 
 - If the image is bad (blurry), click Delete, the file will be deleted (without confirmation).

You can define default folder for folder selection dialog at startup via parameter in command line. Default folder c:\fakes\
