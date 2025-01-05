Overview:
Speedrun Bingo is a meta-game activity where players compete to complete randomly generated tasks within an existing game that players are profficient in.  A 5x5 board of tasks of varying scope and difficulty is generated and players then race to claim more tasks than their opponent by being the first player to complete each task.  

This utility allows users to create a list of tasks and their associated properties (e.g., description, difficulty, ranges for generated values, etc.), and then to concurrently track the progress of which tasks have been claimed by which players.  


Technical Features:

-Players can connect to other desired players by providing a shared room-ID

-Players can provide a .tasks file and then generate a board of tasks randomly selected from that file.  
* The size of the board can be specified (from a 2x2 board to a 7x7 board)
* The average difficulty of the randomly generated tasks can be specified
* The possible variance between the specified average difficulty and the most extreme difficulty of randomly generated tasks
* A randomization seed can be provided (if one isn't provided, a random one is used)

-Players can interact with tasks on the board in the following ways:
* Left Clicking on a task will increment your progress on the task (completing single-step tasks or incrementing a task with X/Y progress to X+1/Y)
* Shift+Left Clicking a task will fully complete single- or multi-step tasks
* Ctrl+Left Clicking will open up any URL attached to the task (it can be helpful to add links to wiki pages to serve as reminders for complicated or niche tasks)
* Right Clicking a task will decrement progress on the task (un-completing single-step tasks or decrementing a task with X/Y progress to X-1/Y)

-Completing a task will claim it by shading in that task with that Player's colour.  Currently, any number of players can claim the same task, but 'locking out' a task when the first player completes it is a mode to be added in the future.  

-Tasks are supplied in a plaintext file and can be customized with various parameters.  Consider the example:
"Desc:{0} Side Quests,Value:10-25,Diff:30-70,Max-count:2,Min-delta:4,URL:game8.co/games/LoZ-BotW/archives/292467#hm_2,Freq:2"
* A Description of the task (to complete "X Side Quests")
* A minimum and maximum value that define what ranges a randomized value can take in the task (X can randomly selected to be between 10 and 25)
* A range of difficulties associated with task (linearly interpolated between 30 and 70 based on which random value is selected between 10 and 25)
* At most 2 variations of this task can appear on the Bingo board (with different randomized values)
* The randomized values for different variations of this task must differ by at least 4 (so you can disallow "12 Side Quests" and "13 Side Quests" since they aren't meaningfully different from one another)
* A link to a webpage that has helpful reminders/explanations about the task
* A frequency modifier that ensures this task occurs roughly 2x as often as a standard task

-Each player can choose a name and a colour to identify their claimed tasks

-A count for each player is displayed for their total number of completed tasks and their total number of lines of completed tasks

-Players can create a save file for the match which saves all of the parameters and progress of the game (players customization and task completion progress).  This save file can then be loaded again to resume the match later.  
